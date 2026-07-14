package com.nikolaandric.daretodance.theme

import android.app.UiModeManager
import android.content.Context
import android.os.Build
import androidx.compose.runtime.Composable
import androidx.compose.ui.platform.LocalContext

@Composable
actual fun systemContrast(): Contrast {
    if (Build.VERSION.SDK_INT < Build.VERSION_CODES.UPSIDE_DOWN_CAKE) return Contrast.Standard
    val uiModeManager =
        LocalContext.current.getSystemService(Context.UI_MODE_SERVICE) as UiModeManager
    return when {
        uiModeManager.contrast >= 0.66f -> Contrast.High
        uiModeManager.contrast >= 0.33f -> Contrast.Medium
        else -> Contrast.Standard
    }
}
