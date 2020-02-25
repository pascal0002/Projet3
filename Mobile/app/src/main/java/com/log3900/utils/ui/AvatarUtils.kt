package com.log3900.utils.ui

import android.content.res.Resources
import androidx.core.content.res.ResourcesCompat
import com.log3900.R
import com.log3900.user.Account
import com.log3900.user.AccountRepository

/**
 * Returns the ID of the pictureID from the corresponding index
 * @param index The picture index (from [1..16] included)
 * @return avatar_1 if index out of range
 */
fun getAvatarID(index: Int): Int {
    if (index > 0 && index < 17) {
        return R.mipmap::class.java.getField("avatar_${index}").getInt(null)
    }

    return R.mipmap.avatar_1
}

/**
 * Returns the ID of the given [Account]'s pictureID linking to the corresponding mipmap
 * @sample avatarView.setImageResource(getAccountAvatarID(myAccount))
 */
fun getAccountAvatarID(account: Account): Int = getAvatarID(account.pictureID)

/**
 * Returns the ID of the current [Account]'s avatar
 */
fun getCurrentAccountAvatarID(): Int {
    val current = AccountRepository.getAccount()
    return getAccountAvatarID(current)
}