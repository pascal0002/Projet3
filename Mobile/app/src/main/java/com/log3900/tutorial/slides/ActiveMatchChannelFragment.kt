package com.log3900.tutorial.slides

import android.os.Bundle
import android.view.LayoutInflater
import android.view.View
import android.view.ViewGroup
import com.log3900.R
import com.log3900.tutorial.TutorialActivity
import com.log3900.tutorial.TutorialSlideFragment

class ActiveMatchChannelFragment : TutorialSlideFragment() {
    override fun onCreateView(
        inflater: LayoutInflater,
        container: ViewGroup?,
        savedInstanceState: Bundle?
    ): View? {
        val root = inflater.inflate(R.layout.fragment_tutorial_slide_active_match_channel_slide, container, false)
        return root
    }

    override fun onResume() {
        super.onResume()
        (activity as TutorialActivity).setSlideTitle(R.string.tutorial_slide_active_match_channel_title)
    }
}