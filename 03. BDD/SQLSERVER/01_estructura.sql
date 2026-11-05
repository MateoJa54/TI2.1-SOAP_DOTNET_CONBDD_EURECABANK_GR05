-- =============================================================================
-- 01_estructura.sql - EURECABANK / SQL Server
-- Crea las 17 tablas del contrato en la base indicada por la variable esquema.
-- Ejecutar con sqlcmd (lo hace cargar_bdd_sqlserver.ps1). Generado a partir de
-- 03. BDD\docker\init\01_estructura.sql (MySQL) y revisado a mano.
-- =============================================================================
USE [$(esquema)];
GO

CREATE TABLE moneda (
    chr_monecodigo char(2) NOT NULL,
    vch_monedescripcion varchar(20) NOT NULL,
    PRIMARY KEY (chr_monecodigo)
);
GO

CREATE TABLE empleado (
    chr_emplcodigo char(4) NOT NULL,
    vch_emplpaterno varchar(25) NOT NULL,
    vch_emplmaterno varchar(25) NOT NULL,
    vch_emplnombre varchar(30) NOT NULL,
    vch_emplciudad varchar(30) NOT NULL,
    vch_empldireccion varchar(50) DEFAULT NULL,
    PRIMARY KEY (chr_emplcodigo)
);
GO

CREATE TABLE sucursal (
    chr_sucucodigo char(3) NOT NULL,
    vch_sucunombre varchar(50) NOT NULL,
    vch_sucuciudad varchar(30) NOT NULL,
    vch_sucudireccion varchar(50) DEFAULT NULL,
    int_sucucontcuenta int NOT NULL,
    PRIMARY KEY (chr_sucucodigo)
);
GO

CREATE TABLE cliente (
    chr_cliecodigo char(5) NOT NULL,
    vch_cliepaterno varchar(25) NOT NULL,
    vch_cliematerno varchar(25) NOT NULL,
    vch_clienombre varchar(30) NOT NULL,
    chr_cliedni char(8) NOT NULL,
    vch_clieciudad varchar(30) NOT NULL,
    vch_cliedireccion varchar(50) NOT NULL,
    vch_clietelefono varchar(20) DEFAULT NULL,
    vch_clieemail varchar(50) DEFAULT NULL,
    PRIMARY KEY (chr_cliecodigo)
);
GO

CREATE TABLE modulo (
    int_moducodigo int NOT NULL,
    vch_modunombre varchar(50) DEFAULT NULL,
    vch_moduestado varchar(15) NOT NULL DEFAULT 'ACTIVO',
    PRIMARY KEY (int_moducodigo),
    CONSTRAINT modulo_chk_1 CHECK (vch_moduestado in ('ACTIVO','ANULADO','CANCELADO'))
);
GO

CREATE TABLE tipomovimiento (
    chr_tipocodigo char(3) NOT NULL,
    vch_tipodescripcion varchar(40) NOT NULL,
    vch_tipoaccion varchar(10) NOT NULL,
    vch_tipoestado varchar(15) NOT NULL DEFAULT 'ACTIVO',
    PRIMARY KEY (chr_tipocodigo),
    CONSTRAINT chk_tipomovimiento_vch_tipoaccion CHECK (vch_tipoaccion in ('INGRESO','SALIDA')),
    CONSTRAINT chk_tipomovimiento_vch_tipoestado CHECK (vch_tipoestado in ('ACTIVO','ANULADO','CANCELADO'))
);
GO

CREATE TABLE usuario (
    chr_emplcodigo char(4) NOT NULL,
    vch_emplusuario varchar(20) NOT NULL,
    vch_emplclave varchar(64) NOT NULL,
    vch_emplestado varchar(15) DEFAULT 'ACTIVO',
    PRIMARY KEY (chr_emplcodigo),
    CONSTRAINT usuario_ibfk_1 FOREIGN KEY (chr_emplcodigo) REFERENCES empleado (chr_emplcodigo),
    CONSTRAINT usuario_chk_1 CHECK (vch_emplestado in ('ACTIVO','ANULADO','CANCELADO'))
);
GO

CREATE TABLE parametro (
    chr_paracodigo char(3) NOT NULL,
    vch_paradescripcion varchar(50) NOT NULL,
    vch_paravalor varchar(70) NOT NULL,
    vch_paraestado varchar(15) NOT NULL DEFAULT 'ACTIVO',
    PRIMARY KEY (chr_paracodigo),
    CONSTRAINT chk_parametro_vch_paraestado CHECK (vch_paraestado in ('ACTIVO','ANULADO','CANCELADO'))
);
GO

CREATE TABLE contador (
    vch_conttabla varchar(30) NOT NULL,
    int_contitem int NOT NULL,
    int_contlongitud int NOT NULL,
    PRIMARY KEY (vch_conttabla)
);
GO

CREATE TABLE permiso (
    chr_emplcodigo char(4) NOT NULL,
    int_moducodigo int NOT NULL,
    vch_permestado varchar(15) NOT NULL DEFAULT 'ACTIVO',
    PRIMARY KEY (chr_emplcodigo, int_moducodigo),
    CONSTRAINT permiso_ibfk_1 FOREIGN KEY (int_moducodigo) REFERENCES modulo (int_moducodigo),
    CONSTRAINT permiso_ibfk_2 FOREIGN KEY (chr_emplcodigo) REFERENCES usuario (chr_emplcodigo),
    CONSTRAINT permiso_chk_1 CHECK (vch_permestado in ('ACTIVO','ANULADO','CANCELADO'))
);
GO

CREATE TABLE asignado (
    chr_asigcodigo char(6) NOT NULL,
    chr_sucucodigo char(3) NOT NULL,
    chr_emplcodigo char(4) NOT NULL,
    dtt_asigfechaalta date NOT NULL,
    dtt_asigfechabaja date DEFAULT NULL,
    PRIMARY KEY (chr_asigcodigo),
    CONSTRAINT fk_asignado_empleado FOREIGN KEY (chr_emplcodigo) REFERENCES empleado (chr_emplcodigo),
    CONSTRAINT fk_asignado_sucursal FOREIGN KEY (chr_sucucodigo) REFERENCES sucursal (chr_sucucodigo)
);
GO

CREATE TABLE cargomantenimiento (
    chr_monecodigo char(2) NOT NULL,
    dec_cargMontoMaximo decimal(12,2) NOT NULL,
    dec_cargImporte decimal(12,2) NOT NULL,
    PRIMARY KEY (chr_monecodigo),
    CONSTRAINT fk_cargomantenimiento_moneda FOREIGN KEY (chr_monecodigo) REFERENCES moneda (chr_monecodigo)
);
GO

CREATE TABLE costomovimiento (
    chr_monecodigo char(2) NOT NULL,
    dec_costimporte decimal(12,2) NOT NULL,
    PRIMARY KEY (chr_monecodigo),
    CONSTRAINT fk_costomovimiento_moneda FOREIGN KEY (chr_monecodigo) REFERENCES moneda (chr_monecodigo)
);
GO

CREATE TABLE interesmensual (
    chr_monecodigo char(2) NOT NULL,
    dec_inteimporte decimal(12,2) NOT NULL,
    PRIMARY KEY (chr_monecodigo),
    CONSTRAINT fk_interesmensual_moneda FOREIGN KEY (chr_monecodigo) REFERENCES moneda (chr_monecodigo)
);
GO

CREATE TABLE cuenta (
    chr_cuencodigo char(8) NOT NULL,
    chr_monecodigo char(2) NOT NULL,
    chr_sucucodigo char(3) NOT NULL,
    chr_emplcreacuenta char(4) NOT NULL,
    chr_cliecodigo char(5) NOT NULL,
    dec_cuensaldo decimal(12,2) NOT NULL,
    dtt_cuenfechacreacion date NOT NULL,
    vch_cuenestado varchar(15) NOT NULL DEFAULT 'ACTIVO',
    int_cuencontmov int NOT NULL,
    chr_cuenclave char(6) NOT NULL,
    PRIMARY KEY (chr_cuencodigo),
    CONSTRAINT fk_cuenta_cliente FOREIGN KEY (chr_cliecodigo) REFERENCES cliente (chr_cliecodigo),
    CONSTRAINT fk_cuenta_moneda FOREIGN KEY (chr_monecodigo) REFERENCES moneda (chr_monecodigo),
    CONSTRAINT fk_cuenta_sucursal FOREIGN KEY (chr_sucucodigo) REFERENCES sucursal (chr_sucucodigo),
    CONSTRAINT fk_cuente_empleado FOREIGN KEY (chr_emplcreacuenta) REFERENCES empleado (chr_emplcodigo),
    CONSTRAINT chk_cuenta_chr_cuenestado CHECK (vch_cuenestado in ('ACTIVO','ANULADO','CANCELADO'))
);
GO

CREATE TABLE movimiento (
    chr_cuencodigo char(8) NOT NULL,
    int_movinumero int NOT NULL,
    dtt_movifecha date NOT NULL,
    chr_emplcodigo char(4) NOT NULL,
    chr_tipocodigo char(3) NOT NULL,
    dec_moviimporte decimal(12,2) NOT NULL,
    chr_cuenreferencia char(8) DEFAULT NULL,
    PRIMARY KEY (chr_cuencodigo, int_movinumero),
    CONSTRAINT fk_movimiento_cuenta FOREIGN KEY (chr_cuencodigo) REFERENCES cuenta (chr_cuencodigo),
    CONSTRAINT fk_movimiento_empleado FOREIGN KEY (chr_emplcodigo) REFERENCES empleado (chr_emplcodigo),
    CONSTRAINT fk_movimiento_tipomovimiento FOREIGN KEY (chr_tipocodigo) REFERENCES tipomovimiento (chr_tipocodigo),
    CONSTRAINT chk_Movimiento_importe4 CHECK (dec_moviimporte >= 0.0)
);
GO

CREATE TABLE logsession (
    ID int NOT NULL IDENTITY(1,1),
    chr_emplcodigo char(4) NOT NULL,
    fec_ingreso datetime NOT NULL,
    fec_salida datetime DEFAULT NULL,
    ip varchar(20) NOT NULL DEFAULT 'NONE',
    hostname varchar(100) NOT NULL DEFAULT 'NONE',
    PRIMARY KEY (ID),
    CONSTRAINT fk_log_session_empleado FOREIGN KEY (chr_emplcodigo) REFERENCES empleado (chr_emplcodigo)
);
GO

-- Indices secundarios (las claves foraneas ya quedaron en cada tabla)
CREATE UNIQUE INDEX U_Usuario_vch_emplusuario ON usuario (vch_emplusuario);
CREATE INDEX FK_Permiso_Modulo ON permiso (int_moducodigo);
CREATE INDEX idx_asignado01 ON asignado (chr_emplcodigo);
CREATE INDEX idx_asignado02 ON asignado (chr_sucucodigo);
CREATE INDEX idx_cargomantenimiento01 ON cargomantenimiento (chr_monecodigo);
CREATE INDEX idx_costomovimiento ON costomovimiento (chr_monecodigo);
CREATE INDEX idx_interesmensual01 ON interesmensual (chr_monecodigo);
CREATE INDEX idx_cuenta01 ON cuenta (chr_cliecodigo);
CREATE INDEX idx_cuenta02 ON cuenta (chr_emplcreacuenta);
CREATE INDEX idx_cuenta03 ON cuenta (chr_sucucodigo);
CREATE INDEX idx_cuenta04 ON cuenta (chr_monecodigo);
CREATE INDEX idx_movimiento01 ON movimiento (chr_tipocodigo);
CREATE INDEX idx_movimiento02 ON movimiento (chr_emplcodigo);
CREATE INDEX idx_movimiento03 ON movimiento (chr_cuencodigo);
CREATE INDEX fk_log_session_empleado ON logsession (chr_emplcodigo);
GO
