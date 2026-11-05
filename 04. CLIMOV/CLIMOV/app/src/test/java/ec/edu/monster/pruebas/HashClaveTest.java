package ec.edu.monster.pruebas;

import static org.junit.Assert.assertEquals;

import ec.edu.monster.util.HashClave;
import org.junit.Test;

public class HashClaveTest {
    @Test public void calculaSha256HexadecimalCompatibleConElClienteDeConsola() {
        assertEquals("ba7816bf8f01cfea414140de5dae2223b00361a396177a9cb410ff61f20015ad",
                HashClave.sha256("abc"));
    }
}
