package lobby

import (
	"gitlab.com/jigsawcorp/log3900/internal/services/auth"
	"log"

	"github.com/google/uuid"
	"gitlab.com/jigsawcorp/log3900/model"

	service "gitlab.com/jigsawcorp/log3900/internal/services"
	"gitlab.com/jigsawcorp/log3900/internal/socket"
	"gitlab.com/jigsawcorp/log3900/pkg/cbroadcast"
)

//Lobby service used the manage the groups before the match
type Lobby struct {
	connected cbroadcast.Channel
	close     cbroadcast.Channel

	join  cbroadcast.Channel
	leave cbroadcast.Channel

	groups   *groups
	shutdown chan bool
}

//Init the lobby service
func (l *Lobby) Init() {
	instance = l
	l.shutdown = make(chan bool)
	l.groups = &groups{}
	l.groups.Init()

	l.subscribe()
}

//Start the lobby service
func (l *Lobby) Start() {
	log.Println("[Lobby] -> Starting service")
	go l.listen()
	//TODO include a cleanup for unused groups after x minutes
}

//Shutdown the lobby service
func (l *Lobby) Shutdown() {
	log.Println("[Lobby] -> Closing service")
	l.groups.CleanAllGroups()
	log.Println("[Lobby] -> Remaining groups closed")
	close(l.shutdown)
}

//CreateGroup method used to broadcast a message that a new group was created
func (l *Lobby) CreateGroup(group *model.Group) {
	l.groups.AddGroup(group)
	socketID, err := auth.GetSocketID(group.OwnerID)

	if err == nil {
		l.groups.JoinGroup(socketID, group.ID)

	}
}

func (l *Lobby) listen() {
	defer service.Closed()

	for {
		select {
		case id := <-l.connected:
			log.Printf("[Lobby] -> New session id: %s", id)
			l.groups.RegisterSession(id.(uuid.UUID))
		case id := <-l.close:
			log.Printf("[Lobby] -> Session disconnected id: %s", id)
			l.groups.UnRegisterSession(id.(uuid.UUID))
		case message := <-l.join:
			rawMessage := message.(socket.RawMessageReceived)
			groupID, err := uuid.FromBytes(rawMessage.Payload.Bytes)
			if err == nil {
				l.groups.JoinGroup(rawMessage.SocketID, groupID)
			}
		case message := <-l.leave:
			rawMessage := message.(socket.RawMessageReceived)
			l.groups.QuitGroup(rawMessage.SocketID)
		case <-l.shutdown:
			return
		}
	}
}

func (l *Lobby) subscribe() {
	l.connected, _, _ = cbroadcast.Subscribe(socket.BSocketAuthConnected)
	l.close, _, _ = cbroadcast.Subscribe(socket.BSocketAuthCloseClient)
	l.join, _, _ = cbroadcast.Subscribe(BJoinGroup)
	l.leave, _, _ = cbroadcast.Subscribe(BLeaveGroup)

}