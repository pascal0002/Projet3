package com.log3900.socket

enum class Event(var eventType: Byte) {
    SOCKET_CONNECTION(0),
    SERVER_RESPONSE(1),
    CLIENT_DISCONNECT(2),
    HEALTH_CHECK_SERVER(9),
    HEALTH_CHECK_CLIENT(10),
    MESSAGE_SENT(20),
    MESSAGE_RECEIVED(21),
    JOIN_CHANNEL(22),
    JOINED_CHANNEL(23),
    LEAVE_CHANNEL(24),
    LEFT_CHANNEL(25),
    CREATE_CHANNEL(26),
    CHANNEL_CREATED(27),
    DELETE_CHANNEL(28),
    CHANNEL_DELETED(29)
}

data class Message(var type: Event, var data: ByteArray)