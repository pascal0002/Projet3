package com.log3900.chat

import android.app.Activity
import android.os.Bundle
import android.os.Handler
import android.view.*
import android.widget.Button
import androidx.appcompat.app.ActionBarDrawerToggle
import androidx.appcompat.widget.Toolbar
import androidx.core.view.GravityCompat
import androidx.drawerlayout.widget.DrawerLayout
import androidx.fragment.app.Fragment
import androidx.recyclerview.widget.LinearLayoutManager
import androidx.recyclerview.widget.RecyclerView
import com.google.android.material.textfield.TextInputEditText
import com.log3900.R
import com.log3900.chat.ui.MessageAdapter
import com.log3900.utils.ui.KeyboardHelper
import java.lang.Thread.sleep
import java.util.*


var username = "admin"

class ChatFragment : Fragment() {
    // Services
    private lateinit var messageService: MessageService
    // UI elements
    private lateinit var messagesRecyclerView: RecyclerView
    private lateinit var messagesViewAdapter: MessageAdapter
    private lateinit var viewManager: RecyclerView.LayoutManager
    private lateinit var sendMessageButton: Button
    private lateinit var toolbar: Toolbar
    private lateinit var drawer: DrawerLayout
    private lateinit var toggle: ActionBarDrawerToggle

    override fun onCreateView(inflater: LayoutInflater, container: ViewGroup?, savedInstanceState: Bundle?): View {
        val rootView: View = inflater.inflate(R.layout.fragment_chat, container, false)

        messageService = MessageService()

        setupUiElements(rootView)

        subscribeToEvents()

        return rootView
    }

    override fun onActivityCreated(savedInstanceState: Bundle?) {
        super.onActivityCreated(savedInstanceState)
        toggle.syncState()
    }

    /**
     * Used to assign all variables holding UI elements for this fragment and setup listeners.
     *
     * @param rootView the root view of this fragment
     */
    private fun setupUiElements(rootView: View) {
        viewManager = LinearLayoutManager(activity)

        setupMessagesRecyclerView(rootView)

        sendMessageButton = rootView.findViewById(R.id.fragment_chat_send_message_button)
        sendMessageButton.setOnClickListener { v -> onSendMessageButtonClick(v)}

        setupToolbar(rootView)

        drawer = rootView.findViewById(R.id.fragment_chat_drawer_layout)
        toggle = ActionBarDrawerToggle(activity, drawer, toolbar, R.string.nav_app_bar_open_drawer_description, R.string.nav_app_bar_open_drawer_description)
        toggle.setHomeAsUpIndicator(R.drawable.ic_hamburger_menu)
        //toggle.syncState()
        //toggle.onDrawerClosed()
    }

    private fun setupToolbar(rootView: View) {
        toolbar = rootView.findViewById(R.id.fragment_chat_top_layout)
        toolbar.inflateMenu(R.menu.fragment_chat_top_menu)
        //toolbar.setNavigationIcon(R.drawable.ic_hamburger_menu)
        toolbar.setTitle("General")

        //toolbar.setNavigationOnClickListener {onToolbarNavigationClick()}
    }

    private fun setupMessagesRecyclerView(rootView: View) {
        messagesRecyclerView = rootView.findViewById(R.id.fragment_chat_message_recycler_view)
        messagesViewAdapter = MessageAdapter(LinkedList())
        messagesRecyclerView.apply {
            setHasFixedSize(true)
            layoutManager = viewManager
            adapter = messagesViewAdapter
        }
    }

    /**
     * Subscribes to all events from services this fragment wants to handle.
     *
     */
    private fun subscribeToEvents() {
        messageService.subscribe(MessageEvent.MESSAGE_RECEIVED, Handler{ msg ->
            onNewMessageReceived(msg)
            true
        })
    }

    /**
     * Handles click on the top toolbar navigation icon. Closes and opens the navigation drawer, which contains the channel list.
     *
     */
    private fun onToolbarNavigationClick() {
        if (drawer.isDrawerOpen(GravityCompat.START)) {
            drawer.closeDrawer(Gravity.LEFT)
        } else {
            drawer.openDrawer(Gravity.LEFT)
        }
    }

    private fun onSendMessageButtonClick(v: View) {
        val messageInput: TextInputEditText = v.rootView.findViewById(R.id.fragment_chat_new_message_input)
        val messageText = messageInput.text.toString()
        messageInput.text?.clear()
        messageService.sendMessage(messageText)

        KeyboardHelper.hideKeyboard(activity as Activity)

    }

    /**
     * Handles a new chat message received. Informs the recyclerView of its presence.
     *
     * @param message the chat message received
     */
    private fun onNewMessageReceived(message: android.os.Message) {
        messagesViewAdapter.messages.addLast(message.obj as Message)
        messagesViewAdapter.notifyItemInserted(messagesViewAdapter.messages.size - 1)
        if (!messagesRecyclerView.canScrollVertically(1)) {
            messagesRecyclerView.smoothScrollToPosition(messagesViewAdapter.messages.size - 1)
        }
    }
}