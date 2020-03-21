package com.log3900.game.match

import android.os.Bundle
import android.util.AttributeSet
import android.util.Log
import android.view.LayoutInflater
import android.view.View
import android.view.ViewGroup
import android.widget.LinearLayout
import android.widget.TextView
import androidx.constraintlayout.widget.ConstraintLayout
import androidx.fragment.app.Fragment
import androidx.recyclerview.widget.LinearLayoutManager
import androidx.recyclerview.widget.RecyclerView
import com.daimajia.androidanimations.library.Techniques
import com.daimajia.androidanimations.library.YoYo
import com.log3900.R
import com.log3900.draw.DrawViewFragment
import com.log3900.game.group.Player
import com.log3900.game.match.UI.WordGuessingView
import com.log3900.game.match.UI.WordToDrawView
import org.w3c.dom.Text
import java.util.*
import kotlin.collections.ArrayList
import kotlin.collections.HashMap

class ActiveMatchFragment : Fragment(), ActiveMatchView {
    private var activeMatchPresenter: ActiveMatchPresenter? = null
    private lateinit var playersAdapter: PlayerAdapter
    private lateinit var drawFragment: DrawViewFragment

    // UI
    private lateinit var footer: LinearLayout
    private lateinit var playersRecyclerView: RecyclerView
    private lateinit var toolbar: ConstraintLayout
    private lateinit var remainingTimeTextView: TextView
    private lateinit var roundsTextView: TextView
    private var guessingView: WordGuessingView? = null
    private var wordToDrawView: WordToDrawView? = null


    override fun onCreateView(inflater: LayoutInflater, container: ViewGroup?, savedInstanceState: Bundle?): View? {
        val rootView: View = inflater.inflate(R.layout.fragment_active_match, container, false)

        setupUI(rootView)

        activeMatchPresenter = ActiveMatchPresenter(this)

        return rootView
    }

    private fun setupUI(rootView: View) {
        footer = rootView.findViewById(R.id.fragment_active_match_footer_container)
        playersRecyclerView = rootView.findViewById(R.id.fragment_active_match_recycler_view_player_list)

        setupRecyclerView()

        drawFragment = childFragmentManager.findFragmentById(R.id.fragment_active_match_draw_container) as DrawViewFragment

        toolbar = activity?.findViewById(R.id.toolbar_active_match_outer_container)!!
        remainingTimeTextView = toolbar.findViewById(R.id.toolbar_active_match_text_view_remaining_time)
        roundsTextView = toolbar.findViewById(R.id.toolbar_active_match_text_view_rounds)
    }

    private fun setupRecyclerView() {
        playersAdapter = PlayerAdapter()
        playersRecyclerView.apply {
            setHasFixedSize(true)
            layoutManager = LinearLayoutManager(context)
            adapter = playersAdapter
        }
    }

    override fun setPlayers(players: ArrayList<Player>) {
        playersAdapter.setPlayers(players)
        playersAdapter.notifyDataSetChanged()
    }

    override fun setPlayerStatus(playerID: UUID, statusImageRes: Int) {
        playersAdapter.setPlayerStatusRes(playerID, statusImageRes)
    }

    override fun setPlayerScores(playerScores: HashMap<UUID, Int>) {
        playersAdapter.setPlayerScores(playerScores)
    }

    override fun clearAllPlayerStatusRes() {
        playersAdapter.resetAllImageRes()
        playersAdapter.notifyDataSetChanged()
    }

    override fun setWordToGuessLength(length: Int) {
        guessingView?.setWordLength(length)
    }

    override fun setWordToDraw(word: String) {
        wordToDrawView?.setWordToGuess(word)
    }

    override fun enableDrawFunctions(enable: Boolean, drawingID: UUID?) {
        drawFragment.enableDrawFunctions(enable, drawingID)
    }

    override fun setTimeValue(time: String) {
        remainingTimeTextView.text = time
    }

    override fun setRoundsValue(rounds: String) {
        roundsTextView.text = rounds
    }

    override fun clearCanvas() {
        drawFragment.clearCanvas()
    }

    override fun showWordGuessingView() {
        if (wordToDrawView != null) {
            footer.removeAllViews()
            wordToDrawView = null
        }

        if (guessingView == null) {
            guessingView = WordGuessingView(context!!)
            guessingView?.layoutParams = ConstraintLayout.LayoutParams(ViewGroup.LayoutParams.MATCH_PARENT, ViewGroup.LayoutParams.MATCH_PARENT)
            footer.addView(guessingView)
            guessingView?.listener = object : WordGuessingView.Listener {
                override fun onGuessPressed(text: String) {
                    activeMatchPresenter?.guessPressed(text)
                }
            }
        }
    }

    override fun showWordToDrawView() {
        if (guessingView != null) {
            footer.removeAllViews()
            guessingView = null
        }

        if (wordToDrawView == null) {
            wordToDrawView = WordToDrawView(context!!)
            wordToDrawView?.layoutParams = ConstraintLayout.LayoutParams(ViewGroup.LayoutParams.MATCH_PARENT, ViewGroup.LayoutParams.MATCH_PARENT)
            footer.addView(wordToDrawView)
        }
    }

    override fun notifyPlayersChanged() {
        playersAdapter.notifyDataSetChanged()
    }

    override fun hideCanvas() {
        YoYo.with(Techniques.RotateOutUpRight)
            .duration(1000)
            .playOn(drawFragment.view)
    }

    override fun showCanvas() {
        YoYo.with(Techniques.RotateInDownLeft)
            .duration(1000)
            .playOn(drawFragment.view)
    }

    override fun showConfetti() {
        drawFragment.showConfetti()
    }

    override fun pulseRemainingTime() {
        YoYo.with(Techniques.Pulse)
            .duration(500)
            .playOn(remainingTimeTextView)
    }

    override fun onDestroy() {
        activeMatchPresenter?.destroy()
        activeMatchPresenter = null
        super.onDestroy()
    }
}