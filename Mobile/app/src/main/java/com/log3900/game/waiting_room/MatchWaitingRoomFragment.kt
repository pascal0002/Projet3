package com.log3900.game.waiting_room

import android.os.Bundle
import android.view.LayoutInflater
import android.view.View
import android.view.ViewGroup
import androidx.fragment.app.Fragment
import com.log3900.R

class MatchWaitingRoomFragment : Fragment(), MatchWaitingRoomView {
    private lateinit var matchWaitingRoomPresenter: MatchWaitingRoomPresenter
    override fun onCreateView(inflater: LayoutInflater, container: ViewGroup?, savedInstanceState: Bundle?): View? {
        val rootView: View = inflater.inflate(R.layout.fragment_match_waiting_room, container, false)
        
        matchWaitingRoomPresenter = MatchWaitingRoomPresenter(this)

        return rootView
    }
}