package ec.edu.monster.ws;

import ec.edu.monster.modelo.RespuestaSoap;
import java.io.ByteArrayInputStream;
import java.nio.charset.StandardCharsets;
import java.util.ArrayList;
import java.util.LinkedHashMap;
import java.util.List;
import java.util.Map;
import javax.xml.parsers.DocumentBuilderFactory;
import org.w3c.dom.Element;
import org.w3c.dom.Node;
import org.w3c.dom.NodeList;
import javax.xml.parsers.ParserConfigurationException;

/** Construye sobres SOAP 1.1 y convierte el elemento return del contrato. */
public final class SoapXml {
    public static final String NAMESPACE = "http://ws.monster.edu.ec/";
    private SoapXml() { }

    public static String envelope(String operation, LinkedHashMap<String, String> parameters) {
        StringBuilder xml = new StringBuilder("<?xml version=\"1.0\" encoding=\"UTF-8\"?>")
                .append("<soap:Envelope xmlns:soap=\"http://schemas.xmlsoap.org/soap/envelope/\">")
                .append("<soap:Body><tns:").append(operation).append(" xmlns:tns=\"")
                .append(NAMESPACE).append("\">");
        for (Map.Entry<String, String> parameter : parameters.entrySet()) {
            // elementFormDefault="unqualified": los parámetros internos no llevan namespace.
            xml.append("<").append(parameter.getKey()).append(">")
                    .append(escape(parameter.getValue())).append("</")
                    .append(parameter.getKey()).append(">");
        }
        return xml.append("</tns:").append(operation)
                .append("></soap:Body></soap:Envelope>").toString();
    }

    public static RespuestaSoap parse(String xml) throws Exception {
        // No se aceptan DTD ni entidades aunque el parser de Android no implemente
        // alguna de las opciones estándar para deshabilitarlas.
        if (xml == null || xml.toUpperCase(java.util.Locale.ROOT).contains("<!DOCTYPE")
                || xml.toUpperCase(java.util.Locale.ROOT).contains("<!ENTITY"))
            throw new IllegalArgumentException("La respuesta SOAP contiene una declaración XML no permitida.");
        DocumentBuilderFactory factory = DocumentBuilderFactory.newInstance();
        factory.setNamespaceAware(true);
        safeFeature(factory, "http://apache.org/xml/features/disallow-doctype-decl", true);
        safeFeature(factory, "http://xml.org/sax/features/external-general-entities", false);
        safeFeature(factory, "http://xml.org/sax/features/external-parameter-entities", false);
        try { factory.setXIncludeAware(false); } catch (UnsupportedOperationException ignored) { }
        factory.setExpandEntityReferences(false);
        Element root = factory.newDocumentBuilder().parse(
                new ByteArrayInputStream(xml.getBytes(StandardCharsets.UTF_8))).getDocumentElement();
        Element returned = findElement(root, "return");
        if (returned == null) {
            Element fault = findElement(root, "faultstring");
            throw new IllegalArgumentException(fault == null ? "Respuesta SOAP sin elemento return"
                    : fault.getTextContent());
        }
        Map<String, Object> values = readChildren(returned);
        String code = string(values.remove("codigo"));
        String state = string(values.remove("estado"));
        String message = string(values.remove("mensaje"));
        return new RespuestaSoap(code, state, message, values);
    }

    private static void safeFeature(DocumentBuilderFactory factory, String name, boolean value) {
        try { factory.setFeature(name, value); }
        catch (ParserConfigurationException | IllegalArgumentException | UnsupportedOperationException ignored) {
            // El filtro explícito de DTD/entidades de arriba mantiene seguro el XML en Android.
        }
    }

    private static Element findElement(Element parent, String localName) {
        if (localName.equals(parent.getLocalName()) || localName.equals(parent.getNodeName())) return parent;
        NodeList children = parent.getChildNodes();
        for (int i = 0; i < children.getLength(); i++) {
            Node node = children.item(i);
            if (node instanceof Element) {
                Element found = findElement((Element) node, localName);
                if (found != null) return found;
            }
        }
        return null;
    }

    private static Map<String, Object> readChildren(Element parent) {
        Map<String, Object> result = new LinkedHashMap<>();
        NodeList children = parent.getChildNodes();
        for (int i = 0; i < children.getLength(); i++) {
            Node node = children.item(i);
            if (!(node instanceof Element)) continue;
            Element element = (Element) node;
            String name = element.getLocalName() == null ? element.getNodeName() : element.getLocalName();
            Object value = hasElementChild(element) ? readChildren(element) : element.getTextContent().trim();
            if (result.containsKey(name)) {
                Object previous = result.get(name);
                List<Object> values = previous instanceof List ? (List<Object>) previous : new ArrayList<>();
                if (!(previous instanceof List)) values.add(previous);
                values.add(value);
                result.put(name, values);
            } else result.put(name, value);
        }
        return result;
    }

    private static boolean hasElementChild(Element element) {
        NodeList children = element.getChildNodes();
        for (int i = 0; i < children.getLength(); i++) if (children.item(i) instanceof Element) return true;
        return false;
    }

    private static String string(Object value) { return value == null ? "" : String.valueOf(value); }

    private static String escape(String value) {
        if (value == null) return "";
        return value.replace("&", "&amp;").replace("<", "&lt;").replace(">", "&gt;")
                .replace("\"", "&quot;").replace("'", "&apos;");
    }
}
