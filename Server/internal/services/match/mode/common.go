package mode

import (
	"github.com/google/uuid"
	"github.com/jinzhu/gorm"
	match2 "gitlab.com/jigsawcorp/log3900/internal/match"
	"gitlab.com/jigsawcorp/log3900/internal/services/auth"
	"gitlab.com/jigsawcorp/log3900/internal/socket"
	"gitlab.com/jigsawcorp/log3900/model"
	"log"
	"sync"
)

type players struct {
	socketID uuid.UUID
	userID   uuid.UUID
	Username string
	Order    int
	IsCPU    bool
}

type base struct {
	readyMatch  sync.WaitGroup
	readyOnce   map[uuid.UUID]bool
	players     []players
	connections map[uuid.UUID]*players
	info        model.Group
	wordHistory map[string]bool
}

func (b *base) init(connections []uuid.UUID, info model.Group) {
	b.players = make([]players, len(connections))
	b.connections = make(map[uuid.UUID]*players, len(connections))
	b.wordHistory = make(map[string]bool)
	for i := range connections {
		socketID := connections[i]
		userID, _ := auth.GetUserID(socketID)
		//Find the user data in the game info
		var user *model.User
		for j := range info.Users {
			if info.Users[j].ID == userID {
				user = info.Users[j]
			}
		}
		if user != nil && userID != uuid.Nil {
			b.players[i] = players{
				socketID: socketID,
				userID:   userID,
				Username: user.Username,
				IsCPU:    false, //TODO replace this with virtual players
			}
			b.connections[socketID] = &b.players[i]
		}
	}

	b.info = info
	b.readyMatch.Add(len(b.players))

	b.readyOnce = make(map[uuid.UUID]bool)
	for i := range b.players {
		b.readyOnce[b.players[i].socketID] = false
	}
	log.Printf("[Match] -> Init match %s", b.info.ID)
}

//broadcast messages to all users not in parallel
func (b *base) broadcast(message *socket.RawMessage) {
	for i := range b.players {
		socket.SendRawMessageToSocketID(*message, b.players[i].socketID)
	}
	log.Printf("[Match] -> Message %d broadcasted, Match: %s", message.MessageType, b.info.ID)
}

//pbroadcast use to broadcast to all users in parallel
func (b *base) pbroadcast(message *socket.RawMessage) {
	for i := range b.players {
		go socket.SendRawMessageToSocketID(*message, b.players[i].socketID)
	}
	log.Printf("[Match] -> Message %d broadcasted in parallel, Match: %s", message.MessageType, b.info.ID)
}

//Wait for all the clients to be ready
func (b *base) waitForPlayers() {
	//TODO include a timeout in case a client drops the connection to avoid a deadlock
	b.readyMatch.Wait()

	//Send a message to all the clients to advise them that the game is starting
	message := socket.RawMessage{}
	message.MessageType = byte(socket.MessageType.GameStarting)
	b.broadcast(&message)
}

func (b *base) ready(socketID uuid.UUID) {
	if !b.readyOnce[socketID] {
		b.readyMatch.Done()
		b.readyOnce[socketID] = true
	}
}

//getPlayers used to return players needs to be exported by the implementing struct
func (b *base) getPlayers() []match2.Player {
	players := make([]match2.Player, len(b.players))
	for i := range b.players {
		players[i].ID = b.players[i].userID
		players[i].Username = b.players[i].Username
		players[i].IsCPU = b.players[i].IsCPU
	}
	return players
}

func (b *base) findGame() *model.Game {
	word := ""
	watchDog := 0

	var game model.Game
	for word == "" {
		model.DB().Where("difficulty = ? and language = ?", b.info.Difficulty, b.info.Language).Order(gorm.Expr("random()")).First(&game)
		if game.ID != uuid.Nil {
			if _, inList := b.wordHistory[word]; !inList || watchDog >= 10 {
				//Add the word to the list so it does not come up again.
				word = game.Word
				b.wordHistory[word] = true
				return &game
			}
		}
		watchDog++
	}
	return &game
}
