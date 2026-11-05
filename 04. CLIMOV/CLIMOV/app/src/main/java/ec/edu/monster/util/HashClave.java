package ec.edu.monster.util;

import java.nio.charset.StandardCharsets;
import java.security.MessageDigest;

/** Hash compatible con el cliente de consola: SHA-256 en hexadecimal minúsculo. */
public final class HashClave {
    private HashClave() { }
    public static String sha256(String value) {
        try {
            byte[] digest = MessageDigest.getInstance("SHA-256").digest(value.getBytes(StandardCharsets.UTF_8));
            StringBuilder hex = new StringBuilder();
            for (byte item : digest) hex.append(String.format("%02x", item & 0xff));
            return hex.toString();
        } catch (Exception e) { throw new IllegalStateException("SHA-256 no está disponible", e); }
    }
}
