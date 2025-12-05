# UsuariosAPI
UsuariosAPI – Documentación Técnica

 Descripción
UsuariosAPI es una API RESTful desarrollada en ASP.NET Core que gestiona usuarios con autenticación JWT, auditoría completa en base de datos y logging profesional con Serilog.  

 Tecnologías utilizadas
- ASP.NET Core 8.0
- Entity Framework Core (Database First)
- SQL Server
- JWT (JSON Web Tokens) para autenticación
- Serilog para logging en consola y archivos
- Auditoría personalizada en tabla Logs


Estructura principal
- Program.cs → Configuración global de Serilog y servicios.
- UsuarioController → CRUD de usuarios con auditoría y logging.
- LoginController → Registro y login con generación de token JWT.
- LogsController → Consulta de registros de auditoría (solo administradores).
- Modelos → Entidades (Usuario, Log).
- DTO → Objetos de transferencia (UsuarioDTO, RegisterDTO, LoginDTO, UsuarioUpdateDTO).

Seguridad
- Autenticación con JWT.  
- Autorización con [Authorize] y roles (Admin, User).  
- Mensajes genéricos para el cliente → sin fugas de información sensible.  
- Logs técnicos detallados → solo visibles en consola/archivo.

Auditoría
Cada acción del usuario se registra en la tabla Logs:
- Insertar → creación de usuario.  
- Actualizar → modificación de datos.  
- Eliminar → eliminación de usuario.  
- Login / Registro → autenticación y alta de usuarios.  
- Errores → se registran con acción Error y mensaje genérico.
