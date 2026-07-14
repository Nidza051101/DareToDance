package com.nikolaandric.daretodance.theme

import androidx.compose.runtime.Composable

enum class Contrast { Standard, Medium, High }

@Composable
expect fun systemContrast(): Contrast
