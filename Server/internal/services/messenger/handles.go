package messenger

import (
	"log"
	"strings"
	"time"

	"github.com/google/uuid"
	"gitlab.com/jigsawcorp/log3900/internal/services/auth"
	"gitlab.com/jigsawcorp/log3900/internal/socket"
	"gitlab.com/jigsawcorp/log3900/model"
)

type handler struct {
	channelsConnections map[uuid.UUID]map[uuid.UUID]bool //channelID - socketID
}

func (h *handler) init() {
	h.channelsConnections = map[uuid.UUID]map[uuid.UUID]bool{}
	h.channelsConnections[uuid.Nil] = make(map[uuid.UUID]bool)

	//TODO init all the channels rooms in memory
}

func (h *handler) handleMessgeSent(message socket.RawMessageReceived) {
	var messageParsed MessageSent
	timestamp := int(time.Now().Unix())
	if message.Payload.DecodeMessagePack(&messageParsed) == nil {
		//Send to all other connected users
		user, err := auth.GetUser(message.SocketID)
		if err != nil {
			log.Printf("[Messenger] -> %s", err)
		}
		log.Printf("[Messenger] -> Receive: \"%s\" Username: \"%s\" ChannelID: %s", messageParsed.Message, user.Username, messageParsed.ChannelID)
		messageToFoward := MessageReceived{
			ChannelID:  messageParsed.ChannelID,
			SenderID:   user.ID.String(),
			SenderName: user.Username,
			Message:    messageParsed.Message,
			Timestamp:  timestamp,
		}
		rawMessage := socket.RawMessage{}
		if rawMessage.ParseMessagePack(byte(socket.MessageType.MessageReceived), messageToFoward) != nil {
			log.Printf("[Messenger] -> Receive: Can't pack message. Dropping packet!")
			return
		}
		channelID, _ := uuid.Parse(messageParsed.ChannelID)
		for k := range h.channelsConnections[channelID] {
			// Send message to the socket in async way
			go socket.SendRawMessageToSocketID(rawMessage, k)
		}
	} else {
		log.Printf("[Messenger] -> Receive: Wrong data format. Dropping packet!")
	}
}

func (h *handler) handleJoinChannel(message socket.RawMessageReceived) {
	if message.Payload.Length == 16 {
		channelID, err := uuid.FromBytes(message.Payload.Bytes)
		if err != nil {
			channel := model.ChatChannel{}
			model.DB().Model(&channel).Related(&model.User{}, "Users")
			model.DB().Preload("Users").Where("id = ?", channelID).First(&channel)

			if channel.ID != uuid.Nil {
				user, _ := auth.GetUser(message.SocketID)

				joinServer := ChannelJoin{
					UserID:    user.ID.String(),
					Username:  user.Username,
					ChannelID: channel.ID.String(),
					Timestamp: int(time.Now().Unix()),
				}

				rawMessage := socket.RawMessage{}
				if rawMessage.ParseMessagePack(byte(socket.MessageType.UserJoinedChannel), joinServer) != nil {
					log.Printf("[Messenger] -> Join: Can't pack message. Dropping packet!")
					return
				}

				//We can join the channel
				model.DB().Model(&channel).Association("Users").Append(user)
				h.channelsConnections[channel.ID][message.SocketID] = true

				for socketID := range h.channelsConnections[channel.ID] {
					//Check if the user has a session
					go socket.SendRawMessageToSocketID(rawMessage, socketID)
				}
			} else {
				log.Printf("[Messenger] -> Join: Channel UUID not found")
			}
		} else {
			log.Printf("[Messenger] -> Join: Invalid channel UUID")
		}
	}
}

func (h *handler) handleConnect(socketID uuid.UUID) {
	h.channelsConnections[uuid.Nil][socketID] = true

	//TODO Broadcast to all the channels that a new user subscribed
	user, _ := auth.GetUser(socketID)
	joinServer := ChannelJoin{
		UserID:    user.ID.String(),
		Username:  user.Username,
		ChannelID: uuid.Nil.String(),
		Timestamp: int(time.Now().Unix()),
	}

	rawMessage := socket.RawMessage{}
	if rawMessage.ParseMessagePack(byte(socket.MessageType.UserJoinedChannel), joinServer) != nil {
		log.Printf("[Messenger] -> Connect: Can't pack message. Dropping packet!")
		return
	}
	for connectionSocketID := range h.channelsConnections[uuid.Nil] {
		go socket.SendRawMessageToSocketID(rawMessage, connectionSocketID)
	}
}

func (h *handler) handleCreateChannel(message socket.RawMessageReceived) {
	var channelParsed ChannelCreateRequest
	timestamp := int(time.Now().Unix())
	if message.Payload.DecodeMessagePack(&channelParsed) == nil {
		name := channelParsed.ChannelName
		if strings.TrimSpace(name) != "" {
			user, err := auth.GetUser(message.SocketID)
			if err != nil {
				//Check if channel already exists
				var count int64
				model.DB().Where("name = ?", name).Count(&count)
				if count == 0 {
					channel := model.ChatChannel{
						Name: name,
					}
					model.DB().Create(&channel)

					//Create request
					response := ChannelCreateResponse{
						ChannelName: name,
						Username:    user.Username,
						UserID:      user.ID.String(),
						Timestamp:   timestamp,
					}
					rawMessage := socket.RawMessage{}
					if rawMessage.ParseMessagePack(byte(socket.MessageType.UserCreateChannel), response) != nil {
						log.Printf("[Messenger] -> Create: Can't pack message. Dropping packet!")
						return
					}

					for socketID := range h.channelsConnections[uuid.Nil] {
						//Check if the user has a session
						go socket.SendRawMessageToSocketID(rawMessage, socketID)
					}
				} else {
					log.Printf("[Messenger] -> Create: Channel already exists. Dropping packet!")
				}
			} else {
				log.Printf("[Messenger] -> Create: Can't find user. Dropping packet!")
			}
		} else {
			log.Printf("[Messenger] -> Create: Invalid channel name. Dropping packet!")
		}
	} else {
		log.Printf("[Messenger] -> Create: Invalid channel decoding. Dropping packet!")
	}
}

func (h *handler) handleDisconnect(socketID uuid.UUID) {
	delete(h.channelsConnections[uuid.Nil], socketID)
	//TODO send a message to leave
}
