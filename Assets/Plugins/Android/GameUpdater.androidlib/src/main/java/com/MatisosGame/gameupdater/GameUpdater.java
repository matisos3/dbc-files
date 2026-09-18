package com.MatisosGame.gameupdater;

import android.app.Activity;
import android.content.Intent;
import android.net.Uri;
import android.os.Build;
import android.provider.Settings;

import androidx.core.content.FileProvider;

import java.io.File;

public class GameUpdater {

    // =========================================================
    // INSTALL APK
    // =========================================================

    /**
     * Uruchamia instalację pobranego APK.
     *
     * @param activity bieżąca aktywność Unity
     * @param apkPath pełna ścieżka do pobranego APK
     */
    public static void installAPK(Activity activity, String apkPath) {

        if (activity == null) {
            return;
        }

        if (apkPath == null || apkPath.isEmpty()) {
            return;
        }

        File apkFile = new File(apkPath);

        if (!apkFile.exists()) {
            return;
        }

        try {

            Intent intent = new Intent(Intent.ACTION_VIEW);

            Uri apkUri;

            // =================================================
            // ANDROID 7+
            // =================================================

            if (Build.VERSION.SDK_INT >= Build.VERSION_CODES.N) {

                apkUri = FileProvider.getUriForFile(
                        activity,
                        activity.getPackageName() + ".fileprovider",
                        apkFile
                );

                intent.setDataAndType(
                        apkUri,
                        "application/vnd.android.package-archive"
                );

                intent.addFlags(
                        Intent.FLAG_GRANT_READ_URI_PERMISSION
                );

                intent.addFlags(
                        Intent.FLAG_GRANT_WRITE_URI_PERMISSION
                );

                intent.addFlags(
                        Intent.FLAG_ACTIVITY_NEW_TASK
                );

            } else {

                // =============================================
                // STARSZE ANDROIDY
                // =============================================

                apkUri = Uri.fromFile(apkFile);

                intent.setDataAndType(
                        apkUri,
                        "application/vnd.android.package-archive"
                );

                intent.addFlags(
                        Intent.FLAG_ACTIVITY_NEW_TASK
                );
            }

            // =================================================
            // URUCHOMIENIE INSTALATORA
            // =================================================

            activity.startActivity(intent);

        } catch (Exception e) {

            e.printStackTrace();
        }
    }


    // =========================================================
    // UNKNOWN APPS
    // =========================================================

    /**
     * Sprawdza, czy użytkownik zezwolił aplikacji
     * na instalowanie aplikacji z nieznanych źródeł.
     *
     * Android 8.0+
     */
    public static boolean canInstallUnknownApps(Activity activity) {

        if (activity == null) {
            return false;
        }

        if (Build.VERSION.SDK_INT < Build.VERSION_CODES.O) {
            return true;
        }

        return activity.getPackageManager()
                .canRequestPackageInstalls();
    }


    /**
     * Otwiera ustawienia Androida pozwalające użytkownikowi
     * zezwolić tej aplikacji na instalowanie APK.
     */
    public static void openUnknownAppsSettings(Activity activity) {

        if (activity == null) {
            return;
        }

        try {

            if (Build.VERSION.SDK_INT >= Build.VERSION_CODES.O) {

                Intent intent = new Intent(
                        Settings.ACTION_MANAGE_UNKNOWN_APP_SOURCES
                );

                intent.setData(
                        Uri.parse(
                                "package:" + activity.getPackageName()
                        )
                );

                intent.addFlags(
                        Intent.FLAG_ACTIVITY_NEW_TASK
                );

                activity.startActivity(intent);

            } else {

                Intent intent = new Intent(
                        Settings.ACTION_SECURITY_SETTINGS
                );

                intent.addFlags(
                        Intent.FLAG_ACTIVITY_NEW_TASK
                );

                activity.startActivity(intent);
            }

        } catch (Exception e) {

            e.printStackTrace();
        }
    }


    // =========================================================
    // INSTALLED VERSION
    // =========================================================

    /**
     * Zwraca versionCode aktualnie zainstalowanej aplikacji.
     */
    public static String getInstalledVersion(Activity activity) {

        if (activity == null) {
            return "";
        }

        try {

            String packageName =
                    activity.getPackageName();

            android.content.pm.PackageInfo packageInfo =
                    activity.getPackageManager()
                            .getPackageInfo(
                                    packageName,
                                    0
                            );

            if (Build.VERSION.SDK_INT >= Build.VERSION_CODES.P) {

                return String.valueOf(
                        packageInfo.getLongVersionCode()
                );

            } else {

                return String.valueOf(
                        packageInfo.versionCode
                );
            }

        } catch (Exception e) {

            e.printStackTrace();

            return "";
        }
    }
}