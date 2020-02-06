package com.log3900.shared.ui

import android.app.AlertDialog
import android.app.Dialog
import android.os.Bundle
import androidx.fragment.app.DialogFragment
import java.lang.IllegalStateException
import android.R
import android.app.Activity
import android.content.Intent
import com.log3900.login.LoginActivity

class WarningDialog : Activity() {
    public override fun onCreate(savedInstanceState: Bundle?) {
        super.onCreate(savedInstanceState)

        AlertDialog.Builder(this)
            .setTitle("Connection Error")
            .setMessage("There was a connection error")
            .setPositiveButton("OK") {dialog, which ->
                val intent = Intent(this, LoginActivity::class.java)
                intent.flags = (Intent.FLAG_ACTIVITY_CLEAR_TOP)
                startActivity(intent)
                finish()
            }
            .setIcon(android.R.drawable.ic_dialog_alert)
            .show()
    }
}