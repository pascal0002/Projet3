package mode

import (
	"github.com/google/uuid"
	"gitlab.com/jigsawcorp/log3900/internal/socket"
	"gitlab.com/jigsawcorp/log3900/model"
	"log"
	"sync"
	"time"
)

const numberOfChances = 3

//Coop represent a cooperative game mode
type Coop struct {
	base
	wordHistory      map[string]bool
	nbVirtualPlayers int
	orderVirtual     []*players
	chances          map[*players]int
	isRunning        bool
	currentWord      string

	remainingTime int

	receiving sync.Mutex
	timeStart time.Time
}

//Init creates the coop game mode
func (c *Coop) Init(connections []uuid.UUID, info model.Group) {
	c.init(connections, info)

	c.chances = make(map[*players]int)
	c.isRunning = true
	c.computeDifficulty()
	c.computeOrder()
}

//computeOrder used to compute the order for the coop
func (c *Coop) computeOrder() {
	c.nbVirtualPlayers = 0

	//Count the number of virtualplayers
	for i := range c.players {
		if c.players[i].IsCPU {
			c.orderVirtual[c.nbVirtualPlayers] = &c.players[i]
			c.nbVirtualPlayers++
		} else {
			c.chances[&c.players[i]] = numberOfChances
		}
	}
}

//computeDifficulty determine the time associated with the game
func (c *Coop) computeDifficulty() {
	//Determine the time based of the difficulty
	switch c.info.Difficulty {
	case 0:
		c.remainingTime = 300
	case 1:
		c.remainingTime = 240
	case 2:
		c.remainingTime = 180
	case 3:
		c.remainingTime = 120
	}
	c.remainingTime *= 1000
}

//Ready client register to make sure they are ready to start the game
func (c *Coop) Ready(socketID uuid.UUID) {
	defer c.receiving.Unlock()
	c.receiving.Lock()

	c.ready(socketID)
}

//Start the game and the game loop
func (c *Coop) Start() {
	c.waitForPlayers()

	//We can start the game loop
	log.Printf("[Match] [Coop] -> Starting gameloop Match: %s", g.info.ID)
	c.timeStart = time.Now()
	for c.isRunning {
		c.GameLoop()
	}
	c.finish()
}

//Disconnect handle disconnect for the coop
func (c *Coop) Disconnect(socketID uuid.UUID) {
	panic("implement me")
}

//TryWord handle when a client wants to try a word
func (c *Coop) TryWord(socketID uuid.UUID, word string) {
	panic("implement me")
}

//IsDrawing method not used by coop
func (c *Coop) IsDrawing(socketID uuid.UUID) {
}

//HintRequested for the current virtual player drawing
func (c *Coop) HintRequested(socketID uuid.UUID) {
	panic("implement me")
}

//Close method used to force close the current game
func (c *Coop) Close() {
	panic("implement me")
}

//GetConnections method used to return all the connections of the players
func (c *Coop) GetConnections() []uuid.UUID {
	defer c.receiving.Unlock()
	c.receiving.Lock()

	connections := make([]uuid.UUID, 0, len(c.players))
	for i := range c.players {
		if !c.players[i].IsCPU {
			connections = append(connections, c.players[i].socketID)
		}
	}
	return connections
}

//GetWelcome message used for the broadcast of the type of game
func (c *Coop) GetWelcome() socket.RawMessage {
	players := make([]PlayersData, 0, len(c.info.Users))
	for i := range c.info.Users {
		players = append(players, PlayersData{
			UserID:   c.info.Users[i].ID.String(),
			Username: c.info.Users[i].Username,
			Points:   0,
			IsCPU:    false,
		})
	}
	welcome := ResponseGameInfo{
		Players:   players,
		GameType:  2,
		TimeImage: 0,
		Laps:      0,
		TotalTime: 0,
	}
	message := socket.RawMessage{}
	message.ParseMessagePack(byte(socket.MessageType.GameWelcome), welcome)
	return message
}

//GameLoop is called every new round
func (c *Coop) GameLoop() {

}

//finish used to properly finish the coop mode
func (c *Coop) finish() {

}
