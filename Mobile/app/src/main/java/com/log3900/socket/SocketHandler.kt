package com.log3900.socket

import android.os.Handler
import android.os.Looper
import java.io.DataInputStream
import java.io.DataOutputStream
import java.io.EOFException
import java.io.IOException
import java.net.InetAddress
import java.net.InetSocketAddress
import java.net.Socket
import java.net.SocketException
import java.util.concurrent.CountDownLatch
import java.util.concurrent.atomic.AtomicBoolean

enum class Request {
    SEND_MESSAGE,
    START_READING,
    STOP_READING,
    CONNECT,
    DISCONNECT,
    SET_MESSAGE_LISTENER
}

object SocketHandler {
    private lateinit var socket: Socket
    private lateinit var inputStream: DataInputStream
    private lateinit var outputStream: DataOutputStream
    private var requestHandler: Handler? = null
    private var messageReadListener: Handler? = null
    private var connectionErrorListener: Handler? = null
    private var readMessages = AtomicBoolean(false)

    fun connect() {
        socket = Socket("log3900.fsae.polymtl.ca", 5001)
        inputStream = DataInputStream(socket.getInputStream())
        outputStream = DataOutputStream(socket.getOutputStream())
    }

    fun createRequestHandler() {
        val lock = CountDownLatch(1)
        Thread(Runnable {
            Looper.prepare()
            requestHandler = Handler {
                handleRequest(it)
                true
            }
            lock.countDown()
            Looper.loop()
        }).start()
        lock.await()
    }

    fun setMessageReadListener(handler: Handler) {
        messageReadListener = handler
    }

    fun setConnectionErrorListener(handler: Handler) {
        connectionErrorListener = handler
    }

    fun sendRequest(message: android.os.Message) {
        requestHandler?.sendMessage(message)
    }

    private fun onWriteMessage(message: Message) {
        try {
            outputStream.writeByte(message.type.eventType.toInt())
            outputStream.writeShort(message.data.size)
            outputStream.write(message.data)
        } catch (e: IOException) {

        }
    }

    fun onDisconnect() {
        socket.close()
    }

    private fun handleRequest(message: android.os.Message) {
        when (message.what) {
            Request.SEND_MESSAGE.ordinal -> {
                if (message.obj is Message) {
                    onWriteMessage(message.obj as Message)
                }
            }
            Request.START_READING.ordinal -> {
                onReadMessage()
            }
            Request.STOP_READING.ordinal -> {
                onStopReadingMessages()
            }
            Request.DISCONNECT.ordinal -> {
                onDisconnect()
            }
            Request.SET_MESSAGE_LISTENER.ordinal -> {
                if (message.obj is Handler) {
                    messageReadListener = message.obj as Handler
                }
            }
        }
    }

    fun onStopReadingMessages() {
        readMessages.compareAndSet(true, false)
    }

    fun onReadMessage() {
        if (!readMessages.get()) {
            readMessages.compareAndSet(false, true)
            Thread(Runnable {
                while (readMessages.get()) {
                    readMessage()
                }
            }).start()
        }
    }

    fun readMessage() {
        try {
            val typeByte = inputStream.readByte()

            val type = Event.values().find { it.eventType == typeByte }
                ?: throw IllegalArgumentException("Invalid message type")

            val length = inputStream.readShort()

            var values = ByteArray(length.toInt())
            var totalReadBytes = 0
            while (totalReadBytes < length) {
                val amountRead = inputStream.read(values, totalReadBytes, length - totalReadBytes)
                totalReadBytes += amountRead
            }

            val message = Message(type, values)

            if (messageReadListener != null) {
                val msg = android.os.Message()
                msg.obj = message
                messageReadListener?.sendMessage(msg)
            }

        } catch (e: SocketException){
            handlerError()
        } catch (e: EOFException) {
            handlerError()
        }
    }

    private fun handlerError() {
        val message = android.os.Message()
        message.what = SocketEvent.CONNECTION_ERROR.ordinal
        connectionErrorListener?.sendMessage(message)
    }
}