package ec.edu.monster;

import android.app.Activity;
import android.content.res.ColorStateList;
import android.graphics.Color;
import android.graphics.Typeface;
import android.graphics.drawable.GradientDrawable;
import android.os.Bundle;
import android.text.InputType;
import android.view.Gravity;
import android.view.View;
import android.view.inputmethod.EditorInfo;
import android.widget.Button;
import android.widget.EditText;
import android.widget.ImageView;
import android.widget.LinearLayout;
import android.widget.ProgressBar;
import android.widget.ScrollView;
import android.widget.TextView;
import android.widget.Toast;
import ec.edu.monster.controlador.ControladorBanco;
import ec.edu.monster.modelo.RespuestaSoap;
import java.util.ArrayList;
import java.util.List;
import java.util.Map;
import java.util.concurrent.ExecutorService;
import java.util.concurrent.Executors;

/** Pantallas de acceso y operaciones. La comunicación y las reglas siguen en el controlador. */
public class MainActivity extends Activity {
    private static final int NAVY = Color.rgb(17, 48, 78);
    private static final int NAVY_DARK = Color.rgb(12, 37, 63);
    private static final int TEAL = Color.rgb(0, 125, 112);
    private static final int CANVAS = Color.rgb(246, 248, 251);
    private static final int INK = Color.rgb(34, 48, 63);
    private static final int MUTED = Color.rgb(103, 117, 132);
    private static final int LINE = Color.rgb(222, 229, 236);
    private final ExecutorService executor = Executors.newSingleThreadExecutor();
    private LinearLayout content, fields, serviceGrid, operationPanel, historyPanel;
    private EditText endpoint, username, password;
    private TextView result, loginResult, welcome;
    private ProgressBar progress;
    private Button primaryAction;
    private final List<EditText> inputs = new ArrayList<>();
    private ControladorBanco controller;
    private int selectedService = -1;

    private static final String[] SERVICE_TITLES = {
            "Cuentas de cliente", "Consultar cuenta", "Movimientos", "Realizar depósito",
            "Realizar retiro", "Transferir", "Costos de mantenimiento"
    };
    private static final String[] SERVICE_SUBTITLES = {
            "Consultar por cédula", "Saldo y datos", "Historial de cuenta", "Agregar fondos",
            "Retirar fondos", "Entre cuentas", "Consultar cargos"
    };
    private static final int[] SERVICE_ICONS = {
            R.drawable.ic_people, R.drawable.ic_wallet, R.drawable.ic_receipt,
            R.drawable.ic_add_circle, R.drawable.ic_remove_circle, R.drawable.ic_transfer,
            R.drawable.ic_price_check
    };

    @Override public void onCreate(Bundle state) {
        super.onCreate(state);
        getWindow().setStatusBarColor(NAVY_DARK);
        getWindow().setNavigationBarColor(CANVAS);
        getWindow().getDecorView().setSystemUiVisibility(View.SYSTEM_UI_FLAG_LIGHT_NAVIGATION_BAR);
        showLogin();
    }

    private void base() {
        ScrollView scroll = new ScrollView(this);
        scroll.setFillViewport(true);
        scroll.setBackgroundColor(CANVAS);
        content = new LinearLayout(this);
        content.setOrientation(LinearLayout.VERTICAL);
        content.setPadding(dp(22), dp(20), dp(22), dp(28));
        scroll.addView(content);
        setContentView(scroll);
        progress = new ProgressBar(this, null, android.R.attr.progressBarStyleHorizontal);
        progress.setIndeterminate(true);
        progress.setProgressTintList(ColorStateList.valueOf(TEAL));
        progress.setVisibility(View.GONE);
        content.addView(progress, new LinearLayout.LayoutParams(-1, dp(3)));
    }

    private void showLogin() {
        base();
        View hero = new LinearLayout(this);
        LinearLayout heroBox = (LinearLayout) hero;
        heroBox.setOrientation(LinearLayout.VERTICAL);
        heroBox.setPadding(dp(22), dp(24), dp(22), dp(24));
        heroBox.setBackground(rounded(NAVY, 24));
        LinearLayout brandRow = row();
        ImageView bankIcon = icon(R.drawable.ic_bank, Color.WHITE, 30);
        LinearLayout iconBox = new LinearLayout(this);
        iconBox.setGravity(Gravity.CENTER);
        iconBox.setBackground(rounded(Color.argb(35,255,255,255), 15));
        iconBox.addView(bankIcon, new LinearLayout.LayoutParams(dp(52), dp(52)));
        brandRow.addView(iconBox);
        TextView brand = text("EurekaBank", 24, Color.WHITE, true);
        LinearLayout brandText = column();
        brandText.setGravity(Gravity.CENTER_VERTICAL);
        brandText.setPadding(dp(14), 0, 0, 0);
        brandText.addView(brand);
        TextView brandCaption = text("BANCA MÓVIL", 11, Color.rgb(194, 216, 231), true);
        brandCaption.setLetterSpacing(.12f);
        brandText.addView(brandCaption);
        brandRow.addView(brandText, new LinearLayout.LayoutParams(0, -2, 1));
        heroBox.addView(brandRow);
        TextView headline = text("Tus servicios bancarios,\na un solo toque", 23, Color.WHITE, true);
        headline.setPadding(0, dp(23), 0, dp(7));
        heroBox.addView(headline);
        heroBox.addView(text("Conéctate de forma segura al servidor de tu institución.", 14, Color.rgb(216, 228, 238), false));
        LinearLayout.LayoutParams heroParams = new LinearLayout.LayoutParams(-1, -2);
        heroParams.bottomMargin = dp(22);
        content.addView(hero, heroParams);

        LinearLayout card = card();
        card.addView(sectionHeading(R.drawable.ic_lock, "Iniciar sesión", "Ingresa tus credenciales para continuar"));
        label(card, "USUARIO");
        username = edit(card, "", "Usuario del empleado", InputType.TYPE_CLASS_TEXT | InputType.TYPE_TEXT_FLAG_CAP_WORDS);
        username.setSingleLine(true);
        username.setImeOptions(EditorInfo.IME_ACTION_NEXT);
        label(card, "CONTRASEÑA");
        password = edit(card, "", "Contraseña", InputType.TYPE_CLASS_TEXT | InputType.TYPE_TEXT_VARIATION_PASSWORD);
        password.setSingleLine(true);
        password.setImeOptions(EditorInfo.IME_ACTION_DONE);
        label(card, "SERVIDOR");
        endpoint = edit(card, "http://10.0.2.2:50775/WSEUREKABANK.svc", "URL del servicio SOAP",
                InputType.TYPE_CLASS_TEXT | InputType.TYPE_TEXT_VARIATION_URI);
        endpoint.setSingleLine(true);
        endpoint.setTextSize(12);
        endpoint.setHorizontallyScrolling(true);
        endpoint.setText(getPreferences(MODE_PRIVATE).getString("endpoint", endpoint.getText().toString()));
        primaryAction = primaryButton("Iniciar sesión", R.drawable.ic_arrow_forward);
        LinearLayout.LayoutParams actionParams = new LinearLayout.LayoutParams(-1, dp(54));
        actionParams.topMargin = dp(9);
        card.addView(primaryAction, actionParams);
        primaryAction.setOnClickListener(v -> login());
        loginResult = text("", 13, Color.rgb(174, 54, 54), false);
        loginResult.setPadding(0, dp(10), 0, 0);
        card.addView(loginResult);
        content.addView(card);

        LinearLayout hint = row();
        hint.setGravity(Gravity.CENTER_VERTICAL);
        hint.setPadding(dp(4), dp(16), dp(4), 0);
        hint.addView(icon(R.drawable.ic_info, TEAL, 20));
        TextView hintText = text("Emulador: 10.0.2.2 · Teléfono: usa la IP local de la computadora que ejecuta el servidor.", 12, MUTED, false);
        hintText.setPadding(dp(9), 0, 0, 0);
        hint.addView(hintText, new LinearLayout.LayoutParams(0, -2, 1));
        content.addView(hint);
    }

    private void login() {
        String url = endpoint.getText().toString().trim();
        String user = username.getText().toString().trim();
        String pass = password.getText().toString();
        if (user.isEmpty()) { username.setError("Ingresa tu usuario"); username.requestFocus(); return; }
        if (pass.isEmpty()) { password.setError("Ingresa tu contraseña"); password.requestFocus(); return; }
        if (!(url.startsWith("http://") || url.startsWith("https://"))) { endpoint.setError("Escribe una dirección HTTP válida"); endpoint.requestFocus(); return; }
        try { controller = new ControladorBanco(url); }
        catch (Exception e) { showLoginError(friendly(e)); return; }
        getPreferences(MODE_PRIVATE).edit().putString("endpoint", url).apply();
        setBusy(true, "Conectando…");
        executor.execute(() -> {
            try {
                RespuestaSoap response = controller.autenticar(user, pass);
                runOnUiThread(() -> {
                    setBusy(false, null);
                    if ("OK".equalsIgnoreCase(response.getEstado()) && controller.haySesion()) showBank(response);
                    else showLoginError("[" + response.getCodigo() + "] " + response.getMensaje());
                });
            } catch (Exception e) { runOnUiThread(() -> { setBusy(false, null); showLoginError(friendly(e)); }); }
        });
    }

    private void showLoginError(String message) {
        loginResult.setText(message);
        loginResult.setVisibility(View.VISIBLE);
    }

    private void showBank(RespuestaSoap authentication) {
        base();
        selectedService = -1;
        LinearLayout top = row();
        LinearLayout titleBlock = column();
        titleBlock.addView(text("Hola,", 14, MUTED, false));
        Object employee = authentication.getDatos().get("nombreEmpleado");
        welcome = text(employee == null ? "Bienvenido" : String.valueOf(employee), 22, NAVY, true);
        titleBlock.addView(welcome);
        top.addView(titleBlock, new LinearLayout.LayoutParams(0, -2, 1));
        LinearLayout secure = row();
        secure.setGravity(Gravity.CENTER);
        secure.setPadding(dp(10), dp(7), dp(10), dp(7));
        secure.setBackground(rounded(Color.rgb(226, 243, 239), 40));
        secure.addView(icon(R.drawable.ic_lock, TEAL, 15));
        TextView secureLabel = text("  Sesión segura", 11, TEAL, true);
        secure.addView(secureLabel);
        top.addView(secure);
        LinearLayout.LayoutParams topP = new LinearLayout.LayoutParams(-1, -2);
        topP.setMargins(0, dp(12), 0, dp(20));
        content.addView(top, topP);

        LinearLayout serviceCard = card();
        serviceCard.addView(sectionHeading(R.drawable.ic_grid, "¿Qué necesitas hacer?", "Elige un servicio para continuar"));
        serviceGrid = column();
        serviceCard.addView(serviceGrid);
        buildServiceGrid();
        content.addView(serviceCard);

        operationPanel = card();
        operationPanel.setVisibility(View.GONE);
        content.addView(operationPanel);

        result = text("", 14, MUTED, false);
        result.setPadding(dp(17), dp(17), dp(17), dp(17));
        result.setBackground(rounded(Color.WHITE, 18, LINE));
        result.setGravity(Gravity.TOP | Gravity.START);
        result.setMinHeight(dp(88));
        result.setVisibility(View.GONE);
        LinearLayout.LayoutParams resultP = new LinearLayout.LayoutParams(-1, -2);
        resultP.topMargin = dp(16);
        content.addView(result, resultP);
        historyPanel = column();
        historyPanel.setVisibility(View.GONE);
        LinearLayout.LayoutParams historyP = new LinearLayout.LayoutParams(-1, -2);
        historyP.topMargin = dp(16);
        content.addView(historyPanel, historyP);
        Button logout = secondaryButton("Cerrar sesión", R.drawable.ic_logout);
        LinearLayout.LayoutParams logoutP = new LinearLayout.LayoutParams(-1, dp(50));
        logoutP.topMargin = dp(14);
        content.addView(logout, logoutP);
        logout.setOnClickListener(v -> { logout.setEnabled(false); executeLogout(); });
    }

    private void buildServiceGrid() {
        serviceGrid.removeAllViews();
        for (int rowIndex = 0; rowIndex < SERVICE_TITLES.length; rowIndex += 2) {
            LinearLayout serviceRow = row();
            int first = rowIndex;
            serviceRow.addView(serviceOption(first), serviceWeight());
            int second = rowIndex + 1;
            if (second < SERVICE_TITLES.length) {
                View spacer = new View(this);
                serviceRow.addView(spacer, new LinearLayout.LayoutParams(dp(10), 1));
                serviceRow.addView(serviceOption(second), serviceWeight());
            }
            LinearLayout.LayoutParams rowParams = new LinearLayout.LayoutParams(-1, -2);
            if (rowIndex > 0) rowParams.topMargin = dp(10);
            serviceGrid.addView(serviceRow, rowParams);
        }
    }

    private LinearLayout.LayoutParams serviceWeight() {
        return new LinearLayout.LayoutParams(0, dp(124), 1);
    }

    private View serviceOption(int index) {
        LinearLayout option = column();
        option.setGravity(Gravity.CENTER_VERTICAL);
        option.setPadding(dp(13), dp(13), dp(12), dp(13));
        option.setMinimumHeight(dp(116));
        option.setBackground(serviceBackground(index == selectedService));
        option.setClickable(true);
        option.setFocusable(true);
        option.setContentDescription(SERVICE_TITLES[index] + ". " + SERVICE_SUBTITLES[index]);

        LinearLayout iconLine = row();
        iconLine.setGravity(Gravity.CENTER_VERTICAL);
        LinearLayout iconBox = new LinearLayout(this);
        iconBox.setGravity(Gravity.CENTER);
        iconBox.setBackground(rounded(index == selectedService ? Color.rgb(216, 239, 234) : Color.rgb(232, 243, 245), 11));
        iconBox.addView(icon(SERVICE_ICONS[index], TEAL, 21), new LinearLayout.LayoutParams(dp(38), dp(38)));
        iconLine.addView(iconBox);
        if (index == selectedService) {
            ImageView check = icon(R.drawable.ic_check, TEAL, 18);
            LinearLayout.LayoutParams checkParams = new LinearLayout.LayoutParams(dp(20), dp(20));
            checkParams.leftMargin = dp(7);
            iconLine.addView(check, checkParams);
        }
        option.addView(iconLine);

        TextView title = text(SERVICE_TITLES[index], 14, NAVY, true);
        title.setMaxLines(2);
        title.setPadding(0, dp(10), 0, dp(3));
        option.addView(title);
        TextView subtitle = text(SERVICE_SUBTITLES[index], 11, MUTED, false);
        subtitle.setMaxLines(1);
        option.addView(subtitle);
        option.setOnClickListener(v -> selectService(index));
        return option;
    }

    private android.graphics.drawable.Drawable serviceBackground(boolean selected) {
        GradientDrawable shape = rounded(selected ? Color.rgb(238, 248, 246) : Color.WHITE, 16,
                selected ? TEAL : LINE);
        ColorStateList rippleColor = ColorStateList.valueOf(Color.argb(30, 0, 125, 112));
        return new android.graphics.drawable.RippleDrawable(rippleColor, shape, null);
    }

    private void selectService(int index) {
        selectedService = index;
        buildServiceGrid();
        buildFields(index);
        operationPanel.setVisibility(View.VISIBLE);
        result.setVisibility(View.GONE);
        historyPanel.setVisibility(View.GONE);
    }

    private void buildFields(int serviceIndex) {
        fields = column();
        inputs.clear();
        operationPanel.removeAllViews();
        operationPanel.addView(sectionHeading(SERVICE_ICONS[serviceIndex], SERVICE_TITLES[serviceIndex],
                "Completa los datos para continuar"));
        String[][] prompts = {{"Cédula del cliente"}, {"Código de cuenta"}, {"Código de cuenta"},
                {"Código de cuenta", "Monto"}, {"Código de cuenta", "Monto"},
                {"Cuenta de origen", "Cuenta de destino", "Monto"}, {"Código de cuenta"}};
        for (String prompt : prompts[serviceIndex]) {
            label(fields, prompt.toUpperCase());
            int type = "Monto".equals(prompt)
                    ? InputType.TYPE_CLASS_NUMBER | InputType.TYPE_NUMBER_FLAG_DECIMAL
                    : InputType.TYPE_CLASS_TEXT | InputType.TYPE_TEXT_FLAG_CAP_CHARACTERS;
            EditText field = edit(fields, "", prompt, type);
            field.setSingleLine(true);
            field.setImeOptions(EditorInfo.IME_ACTION_NEXT);
            inputs.add(field);
        }
        if (inputs.size() > 0) inputs.get(inputs.size() - 1).setImeOptions(EditorInfo.IME_ACTION_DONE);
        operationPanel.addView(fields);
        primaryAction = primaryButton("Ejecutar servicio", R.drawable.ic_arrow_forward);
        LinearLayout.LayoutParams runParams = new LinearLayout.LayoutParams(-1, dp(52));
        runParams.topMargin = dp(10);
        operationPanel.addView(primaryAction, runParams);
        primaryAction.setOnClickListener(v -> executeSelected());
    }

    private void executeSelected() {
        if (selectedService < 0) return;
        int selected = selectedService;
        String[] values = new String[inputs.size()];
        for (int i = 0; i < inputs.size(); i++) {
            values[i] = inputs.get(i).getText().toString().trim();
            if (values[i].isEmpty()) { inputs.get(i).setError("Completa este campo"); inputs.get(i).requestFocus(); return; }
        }
        if (selectedService == 3 || selectedService == 4 || selectedService == 5) {
            int amountIndex = selectedService == 5 ? 2 : 1;
            try {
                if (Double.parseDouble(values[amountIndex]) <= 0) throw new NumberFormatException();
            } catch (NumberFormatException e) { inputs.get(amountIndex).setError("Ingresa un monto mayor que cero"); inputs.get(amountIndex).requestFocus(); return; }
        }
        setBusy(true, "Procesando operación…");
        executor.execute(() -> {
            try {
                RespuestaSoap response = controller.ejecutar(selected, values);
                runOnUiThread(() -> {
                    setBusy(false, null);
                    if (!controller.haySesion() && ("AUTH-003".equals(response.getCodigo()) || "AUTH-004".equals(response.getCodigo()))) {
                        Toast.makeText(this, "[" + response.getCodigo() + "] " + response.getMensaje(), Toast.LENGTH_LONG).show();
                        showLogin();
                    } else {
                        showResult(response, selected);
                    }
                });
            } catch (Exception e) { runOnUiThread(() -> { setBusy(false, null); showResultError(friendly(e)); }); }
        });
    }

    private void executeLogout() {
        setBusy(true, "Cerrando sesión…");
        executor.execute(() -> {
            try {
                RespuestaSoap response = controller.cerrarSesion();
                runOnUiThread(() -> { setBusy(false, null); Toast.makeText(this, response.getMensaje(), Toast.LENGTH_SHORT).show(); showLogin(); });
            } catch (Exception e) { runOnUiThread(() -> { setBusy(false, null); Toast.makeText(this, friendly(e), Toast.LENGTH_LONG).show(); showLogin(); }); }
        });
    }

    private String render(RespuestaSoap response) {
        StringBuilder out = new StringBuilder(response.getEstado() == null ? "Resultado" : response.getEstado().toUpperCase())
                .append("\n[").append(response.getCodigo()).append("] ").append(response.getMensaje());
        for (Map.Entry<String, Object> entry : response.getDatos().entrySet()) {
            if ("token".equalsIgnoreCase(entry.getKey())) continue;
            out.append("\n\n").append(pretty(entry.getKey())).append("\n").append(format(entry.getValue(), 0));
        }
        return out.toString();
    }

    /** Presenta el historial como movimientos legibles en tarjetas, conservando los datos del SOAP. */
    private void showResult(RespuestaSoap response, int serviceIndex) {
        Map<String, Object> data = response.getDatos();
        Object raw = data.get("movimientos");
        if (serviceIndex == 2 && "OK".equalsIgnoreCase(response.getEstado())) {
            List<?> movements = raw instanceof List ? (List<?>) raw
                    : raw instanceof Map ? java.util.Collections.singletonList(raw) : java.util.Collections.emptyList();
            result.setVisibility(View.GONE);
            historyPanel.removeAllViews();
            historyPanel.addView(buildMovementHistory(response, movements));
            historyPanel.setVisibility(View.VISIBLE);
            return;
        }
        historyPanel.setVisibility(View.GONE);
        result.setText(render(response));
        result.setVisibility(View.VISIBLE);
    }

    private void showResultError(String message) {
        historyPanel.setVisibility(View.GONE);
        result.setText(message);
        result.setVisibility(View.VISIBLE);
    }

    private View buildMovementHistory(RespuestaSoap response, List<?> movements) {
        LinearLayout history = card();
        LinearLayout heading = row();
        heading.setGravity(Gravity.CENTER_VERTICAL);
        LinearLayout headingIcon = new LinearLayout(this);
        headingIcon.setGravity(Gravity.CENTER);
        headingIcon.setBackground(rounded(Color.rgb(229, 243, 240), 12));
        headingIcon.addView(icon(R.drawable.ic_receipt, TEAL, 22), new LinearLayout.LayoutParams(dp(42), dp(42)));
        heading.addView(headingIcon);
        LinearLayout titles = column();
        titles.setPadding(dp(12), 0, 0, 0);
        titles.addView(text("Historial de movimientos", 17, NAVY, true));
        titles.addView(text(movements.size() + (movements.size() == 1 ? " registro" : " registros"), 12, MUTED, false));
        heading.addView(titles, new LinearLayout.LayoutParams(0, -2, 1));
        history.addView(heading);

        TextView status = text(response.getMensaje(), 12, TEAL, true);
        status.setPadding(0, dp(12), 0, dp(4));
        history.addView(status);
        if (movements.isEmpty()) {
            TextView empty = text("Esta cuenta todavía no tiene movimientos.", 14, MUTED, false);
            empty.setPadding(0, dp(12), 0, dp(4));
            history.addView(empty);
            return history;
        }
        View divider = new View(this);
        divider.setBackgroundColor(LINE);
        LinearLayout.LayoutParams dividerP = new LinearLayout.LayoutParams(-1, dp(1));
        dividerP.topMargin = dp(8);
        dividerP.bottomMargin = dp(8);
        history.addView(divider, dividerP);

        for (int i = 0; i < movements.size(); i++) {
            Object item = movements.get(i);
            if (!(item instanceof Map)) continue;
            @SuppressWarnings("unchecked") Map<String, Object> movement = (Map<String, Object>) item;
            history.addView(movementCard(movement));
            if (i < movements.size() - 1) {
                View gap = new View(this);
                history.addView(gap, new LinearLayout.LayoutParams(1, dp(10)));
            }
        }
        return history;
    }

    private View movementCard(Map<String, Object> movement) {
        String action = field(movement, "tipoAccion");
        boolean income = "INGRESO".equalsIgnoreCase(action);
        int accent = income ? TEAL : Color.rgb(188, 73, 73);
        LinearLayout item = column();
        item.setPadding(dp(14), dp(13), dp(14), dp(13));
        item.setBackground(rounded(Color.rgb(249, 251, 252), 15, LINE));

        LinearLayout top = row();
        top.setGravity(Gravity.CENTER_VERTICAL);
        LinearLayout badge = row();
        badge.setGravity(Gravity.CENTER);
        badge.setPadding(dp(9), dp(5), dp(9), dp(5));
        badge.setBackground(rounded(income ? Color.rgb(226, 243, 239) : Color.rgb(250, 235, 235), 30));
        badge.addView(text(action.isEmpty() ? "MOVIMIENTO" : action, 10, accent, true));
        top.addView(badge);
        TextView date = text(field(movement, "fecha"), 11, MUTED, false);
        date.setGravity(Gravity.RIGHT | Gravity.CENTER_VERTICAL);
        LinearLayout.LayoutParams dateP = new LinearLayout.LayoutParams(0, -2, 1);
        dateP.leftMargin = dp(8);
        top.addView(date, dateP);
        item.addView(top);

        LinearLayout amountRow = row();
        amountRow.setGravity(Gravity.CENTER_VERTICAL);
        TextView title = text(field(movement, "tipoDescripcion"), 14, NAVY, true);
        title.setMaxLines(2);
        amountRow.addView(title, new LinearLayout.LayoutParams(0, -2, 1));
        String amount = field(movement, "importe");
        TextView value = text((income ? "+ " : "− ") + amount, 18, accent, true);
        value.setGravity(Gravity.RIGHT | Gravity.CENTER_VERTICAL);
        amountRow.addView(value);
        LinearLayout.LayoutParams amountP = new LinearLayout.LayoutParams(-1, -2);
        amountP.topMargin = dp(10);
        item.addView(amountRow, amountP);

        LinearLayout meta = row();
        meta.setPadding(0, dp(8), 0, 0);
        meta.addView(metadata("N.º", field(movement, "numero")), new LinearLayout.LayoutParams(0, -2, 1));
        meta.addView(metadata("CÓDIGO", field(movement, "tipoCodigo")), new LinearLayout.LayoutParams(0, -2, 1));
        meta.addView(metadata("EMPLEADO", field(movement, "empleado")), new LinearLayout.LayoutParams(0, -2, 1));
        item.addView(meta);
        String reference = field(movement, "referencia");
        if (!reference.isEmpty()) {
            TextView ref = text("Cuenta: " + reference, 11, MUTED, false);
            ref.setPadding(0, dp(7), 0, 0);
            item.addView(ref);
        }
        return item;
    }

    private LinearLayout metadata(String label, String value) {
        LinearLayout cell = column();
        cell.addView(text(label, 9, MUTED, true));
        cell.addView(text(value.isEmpty() ? "—" : value, 11, INK, false));
        return cell;
    }

    private String field(Map<String, Object> data, String name) {
        Object value = data.get(name);
        return value == null ? "" : String.valueOf(value);
    }

    private String format(Object value, int depth) {
        if (value instanceof Map) {
            StringBuilder out = new StringBuilder();
            for (Map.Entry<?, ?> item : ((Map<?, ?>) value).entrySet()) {
                if ("token".equalsIgnoreCase(String.valueOf(item.getKey()))) continue;
                if (out.length() > 0) out.append('\n');
                out.append("  ".repeat(Math.min(depth + 1, 4))).append(pretty(String.valueOf(item.getKey())))
                        .append(": ").append(format(item.getValue(), depth + 1));
            }
            return out.toString();
        }
        if (value instanceof List) {
            StringBuilder out = new StringBuilder();
            for (Object item : (List<?>) value) { if (out.length() > 0) out.append('\n'); out.append("• ").append(format(item, depth + 1)); }
            return out.toString();
        }
        return String.valueOf(value);
    }

    private String pretty(String key) {
        String spaced = key.replaceAll("([a-z])([A-Z])", "$1 $2").replace('_', ' ');
        return spaced.isEmpty() ? spaced : spaced.substring(0, 1).toUpperCase() + spaced.substring(1);
    }

    private LinearLayout sectionHeading(int iconRes, String title, String subtitle) {
        LinearLayout heading = row();
        heading.setGravity(Gravity.CENTER_VERTICAL);
        ImageView image = icon(iconRes, TEAL, 22);
        LinearLayout iconHolder = new LinearLayout(this);
        iconHolder.setGravity(Gravity.CENTER);
        iconHolder.setBackground(rounded(Color.rgb(229, 243, 240), 12));
        iconHolder.addView(image, new LinearLayout.LayoutParams(dp(42), dp(42)));
        heading.addView(iconHolder);
        LinearLayout copy = column();
        copy.setPadding(dp(12), 0, 0, 0);
        copy.addView(text(title, 17, NAVY, true));
        TextView sub = text(subtitle, 12, MUTED, false);
        sub.setPadding(0, dp(3), 0, 0);
        copy.addView(sub);
        heading.addView(copy, new LinearLayout.LayoutParams(0, -2, 1));
        LinearLayout.LayoutParams p = new LinearLayout.LayoutParams(-1, -2);
        p.bottomMargin = dp(18);
        heading.setLayoutParams(p);
        return heading;
    }

    private LinearLayout card() {
        LinearLayout box = column();
        box.setPadding(dp(18), dp(20), dp(18), dp(18));
        box.setBackground(rounded(Color.WHITE, 20, LINE));
        return box;
    }

    private EditText edit(LinearLayout parent, String value, String hint, int inputType) {
        EditText field = new EditText(this);
        field.setSingleLine(true);
        field.setText(value);
        field.setHint(hint);
        field.setTextSize(15);
        field.setTextColor(INK);
        field.setHintTextColor(Color.rgb(145, 157, 169));
        field.setInputType(inputType);
        field.setPadding(dp(14), 0, dp(14), 0);
        field.setBackground(rounded(Color.rgb(248, 250, 252), 12, LINE));
        LinearLayout.LayoutParams p = new LinearLayout.LayoutParams(-1, dp(52));
        p.bottomMargin = dp(14);
        parent.addView(field, p);
        return field;
    }

    private void label(LinearLayout parent, String value) {
        TextView label = text(value, 11, MUTED, true);
        label.setLetterSpacing(.08f);
        LinearLayout.LayoutParams p = new LinearLayout.LayoutParams(-1, -2);
        p.bottomMargin = dp(7);
        parent.addView(label, p);
    }

    private Button primaryButton(String value, int iconRes) {
        Button button = new Button(this);
        button.setAllCaps(false);
        button.setText(value);
        button.setTextSize(15);
        button.setTypeface(Typeface.create("sans-serif-medium", Typeface.NORMAL));
        button.setTextColor(Color.WHITE);
        button.setCompoundDrawablesWithIntrinsicBounds(0, 0, iconRes, 0);
        button.setCompoundDrawablePadding(dp(10));
        button.setBackground(rounded(TEAL, 14));
        button.setElevation(dp(2));
        return button;
    }

    private Button secondaryButton(String value, int iconRes) {
        Button button = new Button(this);
        button.setAllCaps(false);
        button.setText(value);
        button.setTextSize(14);
        button.setTextColor(NAVY);
        button.setCompoundDrawablesWithIntrinsicBounds(iconRes, 0, 0, 0);
        button.setCompoundDrawablePadding(dp(8));
        button.setBackground(rounded(Color.WHITE, 13, LINE));
        return button;
    }

    private ImageView icon(int resource, int tint, int size) {
        ImageView view = new ImageView(this);
        view.setImageResource(resource);
        view.setImageTintList(ColorStateList.valueOf(tint));
        view.setContentDescription(null);
        return view;
    }

    private TextView text(String value, int size, int color, boolean medium) {
        TextView view = new TextView(this);
        view.setText(value);
        view.setTextSize(size);
        view.setTextColor(color);
        view.setTypeface(Typeface.create(medium ? "sans-serif-medium" : "sans-serif", Typeface.NORMAL));
        view.setIncludeFontPadding(false);
        return view;
    }

    private LinearLayout row() { LinearLayout box = new LinearLayout(this); box.setOrientation(LinearLayout.HORIZONTAL); return box; }
    private LinearLayout column() { LinearLayout box = new LinearLayout(this); box.setOrientation(LinearLayout.VERTICAL); return box; }
    private GradientDrawable rounded(int color, int radiusDp) { return rounded(color, radiusDp, Color.TRANSPARENT); }
    private GradientDrawable rounded(int color, int radiusDp, int stroke) {
        GradientDrawable shape = new GradientDrawable();
        shape.setColor(color);
        shape.setCornerRadius(dp(radiusDp));
        if (stroke != Color.TRANSPARENT) shape.setStroke(dp(1), stroke);
        return shape;
    }

    private void setBusy(boolean busy, String label) {
        if (progress != null) progress.setVisibility(busy ? View.VISIBLE : View.GONE);
        if (primaryAction != null) {
            primaryAction.setEnabled(!busy);
            if (busy && label != null) primaryAction.setText(label);
            else primaryAction.setText(selectedService < 0 ? "Iniciar sesión" : "Ejecutar servicio");
        }
        if (content != null) setChildrenEnabled(content, !busy);
    }

    private void setChildrenEnabled(android.view.ViewGroup group, boolean enabled) {
        for (int i = 0; i < group.getChildCount(); i++) {
            View child = group.getChildAt(i);
            if (child == progress || child == primaryAction) continue;
            child.setEnabled(enabled);
            if (child instanceof android.view.ViewGroup) setChildrenEnabled((android.view.ViewGroup) child, enabled);
        }
    }

    private int dp(int value) { return (int) (value * getResources().getDisplayMetrics().density + 0.5f); }
    private String friendly(Exception e) { return e.getMessage() == null ? "No se pudo comunicar con el servidor." : e.getMessage(); }
    @Override protected void onDestroy() { super.onDestroy(); executor.shutdownNow(); }
}
