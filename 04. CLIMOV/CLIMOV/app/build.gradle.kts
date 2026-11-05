plugins {
    alias(libs.plugins.android.application)
}

android {
    namespace = "ec.edu.monster"
    compileSdk = 37

    defaultConfig {
        applicationId = "ec.edu.monster.eurekabank.soapdotnet"
        minSdk = 24
        targetSdk = 35
        versionCode = 1
        versionName = "1.0"
        testInstrumentationRunner = "androidx.test.runner.AndroidJUnitRunner"
    }

    compileOptions {
        sourceCompatibility = JavaVersion.VERSION_11
        targetCompatibility = JavaVersion.VERSION_11
    }
}

dependencies {
    testImplementation(libs.junit)
}
