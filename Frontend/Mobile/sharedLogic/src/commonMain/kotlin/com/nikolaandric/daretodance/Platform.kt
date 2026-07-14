package com.nikolaandric.daretodance

interface Platform {
    val name: String
}

expect fun getPlatform(): Platform