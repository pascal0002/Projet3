package com.log3900.game.match.coop

import android.animation.Animator
import android.animation.AnimatorSet
import android.animation.ObjectAnimator
import android.animation.PropertyValuesHolder
import android.graphics.Color
import android.os.Bundle
import android.util.Log
import android.view.LayoutInflater
import android.view.View
import android.view.ViewGroup
import android.widget.ImageView
import android.widget.LinearLayout
import android.widget.TextView
import androidx.activity.OnBackPressedCallback
import androidx.recyclerview.widget.LinearLayoutManager
import androidx.recyclerview.widget.RecyclerView
import com.daimajia.easing.linear.Linear
import com.log3900.R
import com.log3900.game.group.Player
import com.log3900.game.match.ActiveMatchFragment
import com.log3900.game.match.PlayerAdapter
import com.log3900.game.match.UI.DrawerHolderView
import com.log3900.game.match.ffa.ActiveFFAMatchPresenter
import com.log3900.game.match.ffa.ActiveFFAMatchView

class ActiveCoopMatchFragment : ActiveMatchFragment(), ActiveCoopMatchView {
    private var activeCoopMatchPresenter: ActiveCoopMatchPresenter? = null
    private lateinit var teamPlayersAdapter: TeamPlayerAdapter

    // UI
    private lateinit var teamScoreTextView: TextView
    private lateinit var teamScoreChangedTextView: TextView
    private lateinit var remainingLivesContainer: LinearLayout
    private lateinit var teamateBadGuessTextView: TextView
    private lateinit var drawerViewHolder: DrawerHolderView
    private var remainingLivesHearts: ArrayList<ImageView> = arrayListOf()
    protected lateinit var teamPlayersRecyclerView: RecyclerView

    override fun onCreateView(inflater: LayoutInflater, container: ViewGroup?, savedInstanceState: Bundle?): View? {
        val rootView: View = inflater.inflate(R.layout.fragment_active_coop_match, container, false)

        setupUI(rootView)

        activeCoopMatchPresenter = ActiveCoopMatchPresenter(this)
        activeMatchPresenter = activeCoopMatchPresenter

        requireActivity().onBackPressedDispatcher.addCallback(this, object : OnBackPressedCallback(true) {
            override fun handleOnBackPressed() {
                onBackButtonPressed()
            }
        })

        return rootView
    }

    override fun setupUI(rootView: View) {
        drawerViewHolder = rootView.findViewById(R.id.fragment_active_coop_match_drawer_holder_view)
        teamateBadGuessTextView = rootView.findViewById(R.id.fragment_active_coop_match_text_view_teamate_bad_guess_warning)
        super.setupUI(rootView)
    }

    override fun setupToolbar(rootView: View) {
        super.setupToolbar(rootView)

        teamScoreTextView = toolbar.findViewById(R.id.toolbar_active_coop_match_text_view_team_score)
        teamScoreChangedTextView = toolbar.findViewById(R.id.toolbar_active_coop_match_text_view_score_changed)
        remainingLivesContainer = toolbar.findViewById(R.id.toolbar_active_coop_match_container_lives)
    }

    override fun setupHumanPlayerRecyclerView(rootView: View) {
        teamPlayersRecyclerView = rootView.findViewById(R.id.fragment_active_coop_match_recycler_view_team_player_list)
        teamPlayersAdapter = TeamPlayerAdapter()
        teamPlayersRecyclerView.apply {
            setHasFixedSize(true)
            layoutManager = LinearLayoutManager(context)
            adapter = teamPlayersAdapter
        }
    }

    override fun setPlayers(players: ArrayList<Player>) {
        teamPlayersAdapter.setPlayers(players)
        teamPlayersAdapter.notifyDataSetChanged()
    }

    override fun notifyPlayersChanged() {
        teamPlayersAdapter.notifyPlayersChanged()
    }

    override fun setTeamScore(score: String) {
        teamScoreTextView.text = score
    }

    override fun setRemainingLives(count: Int) {
        if (count == remainingLivesHearts.size) {
            return
        }

        if (count < remainingLivesHearts.size) {
            removeRemainingLife()
            return
        }

        remainingLivesContainer.removeAllViews()
        remainingLivesHearts.clear()

        for (i in 0 until count) {
            addRemainingLife()
        }
    }

    override fun addRemainingLife() {
        val newHeart = createHeartImage()
        remainingLivesHearts.add(newHeart)
        remainingLivesContainer.addView(newHeart)
    }

    override fun removeRemainingLife() {
        if (remainingLivesHearts.size == 0 || remainingLivesContainer.childCount == 0) {
            remainingLivesContainer.removeAllViews()
            remainingLivesHearts.clear()
            return
        }

        val viewToRemove = remainingLivesHearts[remainingLivesHearts.size - 1]
        remainingLivesHearts.remove(viewToRemove)
        viewToRemove.bringToFront()
        viewToRemove.parent.requestLayout()
        val scaleUpAnimator = ObjectAnimator.ofPropertyValuesHolder(
            viewToRemove,
            PropertyValuesHolder.ofFloat(View.SCALE_X, 1f, 1.8f),
            PropertyValuesHolder.ofFloat(View.SCALE_Y, 1f, 1.8f)
        )
        scaleUpAnimator.duration = 200

        val scaleDownAnimator = ObjectAnimator.ofPropertyValuesHolder(
            viewToRemove,
            PropertyValuesHolder.ofFloat(View.SCALE_X, 1.8f, 0f),
            PropertyValuesHolder.ofFloat(View.SCALE_Y, 1.8f, 0f)
        )
        scaleDownAnimator.duration = 200

        scaleDownAnimator.addListener(object : Animator.AnimatorListener {
            override fun onAnimationEnd(animation: Animator?) {
                remainingLivesContainer.removeViewAt(remainingLivesContainer.childCount - 1)
            }

            override fun onAnimationCancel(animation: Animator?) {
                // I shall not
            }

            override fun onAnimationRepeat(animation: Animator?) {
                // Provide useless implementations
            }

            override fun onAnimationStart(animation: Animator?) {
                // For something I do not need
            }
        })

        val animatorSet = AnimatorSet()
        animatorSet.playSequentially(scaleUpAnimator, scaleDownAnimator)

        animatorSet.start()
    }

    override fun showScoreChangedAnimation(scoreChangedValue: String, isPositive: Boolean) {
        if (isPositive) {
            teamScoreChangedTextView.setTextColor(Color.GREEN)
        } else {
            teamScoreChangedTextView.setTextColor(Color.RED)
        }

        teamScoreChangedTextView.text = scoreChangedValue
        teamScoreChangedTextView.bringToFront()

        val scaleUpAnimator = ObjectAnimator.ofPropertyValuesHolder(
            teamScoreChangedTextView,
            PropertyValuesHolder.ofFloat(View.SCALE_X, 1f, 2f),
            PropertyValuesHolder.ofFloat(View.SCALE_Y, 1f, 2f)
        )
        scaleUpAnimator.duration = 2000

        val alphaChangeAnimator = ObjectAnimator.ofPropertyValuesHolder(
            teamScoreChangedTextView,
            PropertyValuesHolder.ofFloat(View.ALPHA, 0f, 1f)
        )
        alphaChangeAnimator.repeatCount = 1
        alphaChangeAnimator.repeatMode = ObjectAnimator.REVERSE
        alphaChangeAnimator.duration = 1000
        alphaChangeAnimator.start()
        scaleUpAnimator.start()
    }

    override fun setDrawer(player: Player) {
        drawerViewHolder.setDrawer(player)
    }

    override fun animateBadTeamateGuessWarning() {
        teamateBadGuessTextView.setTextColor(Color.RED)
        val alphaChangeAnimator = ObjectAnimator.ofPropertyValuesHolder(
            teamateBadGuessTextView,
            PropertyValuesHolder.ofFloat(View.ALPHA, 0f, 1f)
        )
        alphaChangeAnimator.repeatCount = 1
        alphaChangeAnimator.repeatMode = ObjectAnimator.REVERSE
        alphaChangeAnimator.duration = 1000
        alphaChangeAnimator.start()
    }

    override fun onDestroy() {
        activeCoopMatchPresenter = null
        super.onDestroy()
    }

    private fun createHeartImage(): ImageView {
        val imageView = ImageView(context)
        imageView.setImageResource(R.drawable.ic_heart_red)
        imageView.layoutParams = LinearLayout.LayoutParams(30, 30)

        return imageView
    }
}