package ec.edu.monster.controlador;

import ec.edu.monster.modelo.RespuestaSoap;
import ec.edu.monster.util.HashClave;
import ec.edu.monster.ws.ClienteSoap;
import ec.edu.monster.ws.IServicioSoap;
import java.math.BigDecimal;
import java.util.LinkedHashMap;

/** Coordina la autenticación y las nueve operaciones disponibles en el contrato. */
public final class ControladorBanco {
    public static final String[] OPERACIONES = {
            "Listar cuentas de cliente", "Consultar cuenta", "Consultar movimientos",
            "Realizar depósito", "Realizar retiro", "Realizar transferencia", "Consultar costos de mantenimiento"
    };
    private final IServicioSoap cliente;
    private String token;

    public ControladorBanco(String endpoint) { this(new ClienteSoap(endpoint)); }

    /** Constructor de prueba para inyectar un transporte SOAP falso, sin red. */
    public ControladorBanco(IServicioSoap cliente) { this.cliente = cliente; }

    public RespuestaSoap autenticar(String usuario, String clave) throws Exception {
        LinkedHashMap<String, String> args = new LinkedHashMap<>();
        args.put("usuario", usuario.trim());
        args.put("clave", HashClave.sha256(clave));
        RespuestaSoap response = cliente.llamar("autenticar", args);
        Object receivedToken = response.getDatos().get("token");
        if ("OK".equalsIgnoreCase(response.getEstado()) && receivedToken != null)
            token = String.valueOf(receivedToken);
        return response;
    }

    /** Cierra la sesión activa; se mantiene separado de las operaciones bancarias. */
    public RespuestaSoap cerrarSesion() throws Exception {
        exigirSesion();
        LinkedHashMap<String, String> args = new LinkedHashMap<>();
        args.put("token", token);
        RespuestaSoap response = cliente.llamar("cerrarSesion", args);
        token = null;
        return response;
    }

    public RespuestaSoap ejecutar(int indice, String... input) throws Exception {
        exigirSesion();
        LinkedHashMap<String, String> args = new LinkedHashMap<>();
        String operacion;
        switch (indice) {
            case 0:
                args.put("cedula", value(input, 0)); args.put("token", token);
                operacion = "listarCuentasPorCliente"; break;
            case 1:
                args.put("cuenta", value(input, 0)); args.put("token", token);
                operacion = "consultarCuenta"; break;
            case 2:
                args.put("cuenta", value(input, 0)); args.put("token", token);
                operacion = "consultarMovimientos"; break;
            case 3:
                args.put("cuenta", value(input, 0)); args.put("monto", amount(value(input, 1)));
                args.put("token", token); operacion = "realizarDeposito"; break;
            case 4:
                args.put("cuenta", value(input, 0)); args.put("monto", amount(value(input, 1)));
                args.put("token", token); operacion = "realizarRetiro"; break;
            case 5:
                args.put("cuentaOrigen", value(input, 0)); args.put("cuentaDestino", value(input, 1));
                args.put("monto", amount(value(input, 2))); args.put("token", token);
                operacion = "realizarTransferencia"; break;
            case 6:
                args.put("cuenta", value(input, 0)); args.put("token", token);
                operacion = "consultarCostosMantenimiento"; break;
            default: throw new IllegalArgumentException("Selecciona una operación válida.");
        }
        RespuestaSoap response = cliente.llamar(operacion, args);
        if ("AUTH-003".equals(response.getCodigo()) || "AUTH-004".equals(response.getCodigo())) token = null;
        return response;
    }

    public boolean haySesion() { return token != null && !token.isEmpty(); }
    private void exigirSesion() { if (!haySesion()) throw new IllegalStateException("Inicia sesión para continuar."); }
    private static String value(String[] values, int index) {
        if (index >= values.length || values[index] == null || values[index].trim().isEmpty())
            throw new IllegalArgumentException("Completa todos los campos requeridos.");
        return values[index].trim();
    }
    private static String amount(String value) {
        String normalized = value.trim();
        // Coincide con consola: acepta coma solo cuando no hay punto y deja al servidor
        // decidir si cero, negativos o más de dos decimales cumplen MON-001.
        if (normalized.indexOf('.') < 0) normalized = normalized.replace(',', '.');
        if (!normalized.matches("[-+]?(?:\\d+(?:\\.\\d*)?|\\.\\d+)"))
            throw new IllegalArgumentException("MON-001 · El monto debe ser mayor que cero y con máximo dos decimales");
        try {
            return new BigDecimal(normalized).toPlainString();
        } catch (NumberFormatException e) {
            throw new IllegalArgumentException("MON-001 · El monto debe ser mayor que cero y con máximo dos decimales");
        }
    }
}
