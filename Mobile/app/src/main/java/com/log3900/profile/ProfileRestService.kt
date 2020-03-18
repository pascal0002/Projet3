package com.log3900.profile

import com.google.gson.JsonArray
import com.google.gson.JsonObject
import com.log3900.shared.network.rest.Retrofit
import retrofit2.Call
import retrofit2.Response
import retrofit2.http.*

interface ProfileRestService {

    // Profile modification
    @PUT("users/")
    fun modifyProfile(
        @Header("SessionToken") sessionToken: String, @Header("Language") language: String,
        @Body data: JsonObject
    ): Call<JsonArray>

    // Statistics
    @GET("stats/")
    suspend fun getUserStats(
        @Header("SessionToken") sessionToken: String, @Header("Language") language: String
    ): Response<JsonObject>

    // TODO: userid
    @GET("stats/history/?start=0&end=100")
    suspend fun getHistory(
        @Header("SessionToken") sessionToken: String, @Header("Language") language: String
    ): Response<JsonObject>

    companion object {
        val service = Retrofit.retrofit.create(ProfileRestService::class.java)
    }
}