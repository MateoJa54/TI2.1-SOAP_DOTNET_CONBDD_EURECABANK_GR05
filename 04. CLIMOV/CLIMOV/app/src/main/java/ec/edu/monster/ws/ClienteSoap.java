package ec.edu.monster.ws;

import ec.edu.monster.modelo.RespuestaSoap;
import java.io.ByteArrayOutputStream;
import java.io.InputStream;
import java.io.OutputStream;
import java.net.HttpURLConnection;
import java.net.URL;
import java.nio.charset.StandardCharsets;
import java.util.LinkedHashMap;

/** Adaptador HTTP para el servicio SOAP; cada instancia puede usar otra IP/puerto. */
public final class ClienteSoap implements IServicioSoap {
    private final String endpoint;

    public ClienteSoap(String endpoint) {
        if (endpoint == null || !(endpoint.startsWith("http://") || endpoint.startsWith("https://")))
            throw new IllegalArgumentException("Ingresa una URL HTTP válida del servidor.");
        this.endpoint = endpoint;
    }

    @Override public RespuestaSoap llamar(String operacion, LinkedHashMap<String, String> parametros) throws Exception {
        byte[] body = SoapXml.envelope(operacion, parametros).getBytes(StandardCharsets.UTF_8);
        HttpURLConnection connection = (HttpURLConnection) new URL(endpoint).openConnection();
        connection.setRequestMethod("POST");
        connection.setConnectTimeout(7000);
        connection.setReadTimeout(20000);
        connection.setDoOutput(true);
        connection.setRequestProperty("Content-Type", "text/xml; charset=utf-8");
        connection.setRequestProperty("SOAPAction", "\"\"");
        connection.setFixedLengthStreamingMode(body.length);
        try {
            try (OutputStream output = connection.getOutputStream()) { output.write(body); }
            int status = connection.getResponseCode();
            InputStream response = status >= 400 ? connection.getErrorStream() : connection.getInputStream();
            if (response == null) throw new IllegalStateException("El servidor respondió HTTP " + status);
            String xml;
            try (InputStream input = response; ByteArrayOutputStream bytes = new ByteArrayOutputStream()) {
                byte[] buffer = new byte[4096];
                int count;
                while ((count = input.read(buffer)) != -1) bytes.write(buffer, 0, count);
                xml = new String(bytes.toByteArray(), StandardCharsets.UTF_8);
            }
            return SoapXml.parse(xml);
        } finally { connection.disconnect(); }
    }
}
