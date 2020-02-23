package socket

import (
	"encoding/binary"
	"fmt"

	"github.com/google/uuid"
	"github.com/vmihailenco/msgpack/v4"
)

// Quick implementation of enum to represent different message types
type messageType struct {
	ServerConnection         int
	ServerConnectionResponse int
	ServerDisconnection      int
	UserDisconnected         int
	HealthCheck              int
	HealthCheckResponse      int
	MessageSent              int
	MessageReceived          int
	JoinChannel              int
	UserJoinedChannel        int
	LeaveChannel             int
	UserLeftChannel          int
	CreateChannel            int
	UserCreateChannel        int
	DestroyChannel           int
	UserDestroyedChannel     int
	ErrorResponse            int
}

// MessageType represents the available message types to send to clients.
var MessageType = &messageType{
	ServerConnection:         0,
	ServerConnectionResponse: 1,
	UserDisconnected:         2,
	ServerDisconnection:      3,
	HealthCheck:              9,
	HealthCheckResponse:      10,
	MessageSent:              20,
	MessageReceived:          21,
	JoinChannel:              22,
	UserJoinedChannel:        23,
	LeaveChannel:             24,
	UserLeftChannel:          25,
	CreateChannel:            26,
	UserCreateChannel:        27,
	DestroyChannel:           28,
	UserDestroyedChannel:     29,
	ErrorResponse:            255,
}

// SerializableMessage Represents a serializable message sent over socket
type SerializableMessage struct {
	MessageType int
	Data        interface{}
}

// RawMessage represents a message that will not be serialized and be sent raw
type RawMessage struct {
	MessageType byte
	Length      uint16
	Bytes       []byte
}

//RawMessageReceived represents a message that was received. It is associated with a client id
type RawMessageReceived struct {
	Payload  RawMessage
	SocketID uuid.UUID
}

//errorMessage used to send an error message to the client
type errorMessage struct {
	Type      int
	ErrorCode int
	Message   string
}

// ToBytesSlice converts the raw message into the TLV format
func (message *RawMessage) ToBytesSlice() []byte {
	bytes := make([]byte, message.Length+3)
	bytes[0] = message.MessageType
	binary.BigEndian.PutUint16(bytes[1:], message.Length)
	copy(bytes[3:], message.Bytes)

	return bytes
}

// ParseMessage create the message object
func (message *RawMessage) ParseMessage(typeMessage byte, length uint16, bytes []byte) {
	message.MessageType = typeMessage
	message.Length = length
	message.Bytes = bytes[3:]
}

// ParseMessagePack parse message pack into a RawMessage
func (message *RawMessage) ParseMessagePack(typeMessage byte, v interface{}) error {
	bytes, err := msgpack.Marshal(v)
	if err != nil {
		return err
	}

	if len(bytes) > 65535 {
		return fmt.Errorf("cannot have bytes larger than 65535")
	}
	message.Length = uint16(len(bytes))
	message.MessageType = typeMessage
	message.Bytes = bytes

	return nil
}

// DecodeMessagePack parse the message into MessagePack
func (message *RawMessage) DecodeMessagePack(v interface{}) error {
	err := msgpack.Unmarshal(message.Bytes, v)
	if err != nil {
		return err
	}
	return nil
}