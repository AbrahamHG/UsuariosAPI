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

 [!NOTE]
- Todas las peticiones que requieren autenticación deben incluir el header:
  Authorization: Bearer <token>  `
- El token se obtiene al hacer login en POST /api/login.
- Los endpoints de LogsController están protegidos para usuarios con rol Admin.
  
# Endpoints
## LoginController
* Registro de usuario
`https
POST https://localhost:5001/api/login/register

* Login (obtener token JWT)
`https
POST https://localhost:5001/api/login

* Usuarios – UsuarioController (requiere token en header)
`https
GET https://localhost:5001/api/usuario
Authorization: Bearer <token>

GET https://localhost:5001/api/usuario/1
Authorization: Bearer <token>

* Crear usuario
`https
POST https://localhost:5001/api/usuario
Authorization: Bearer <token>

* Actualizar usuario
`https
PUT https://localhost:5001/api/usuario/1
Authorization: Bearer <token>


* Eliminar usuario
`http
DELETE https://localhost:5001/api/usuario/1
Authorization: Bearer <token>

## Auditoría – LogsController (requiere rol Admin)

* Obtener todos los logs
`https
GET https://localhost:5001/api/logs
Authorization: Bearer <token>

* Obtener logs por usuario
`https
GET https://localhost:5001/api/logs/usuario/1
Authorization: Bearer <token>

* Obtener logs por acción
`http
GET https://localhost:5001/api/logs/accion/Insertar
Authorization: Bearer <token>

 # Ejemplos de Peticiones (Postman/Apidog)

## Autenticación – LoginController

 * Registro de usuario
`https
POST https://localhost:5001/api/login/register
Content-Type: application/json
EJMPLO
{
  "nombre": "Abraham",
  "email": "abraham@test.com",
  "password": "123456",
  "rol": "Admin"
}
`

* Login (obtener token JWT)
`https
POST https://localhost:5001/api/login
EJEMPLO
Content-Type: application/json

{
  "email": "abraham@test.com",
  "password": "123456"
}
`

Respuesta:
`json
{
  "token": "eyJhbGciOiJIUzI1NiIsInR5cCI6..."
}
`
 ## Usuarios – UsuarioController (requiere token en header)

* Obtener todos los usuarios
`https
GET https://localhost:5001/api/usuario
Authorization: Bearer <token>
`

* Obtener usuario por ID
`https
GET https://localhost:5001/api/usuario/1
Authorization: Bearer <token>
`

* Crear usuario
`https
POST https://localhost:5001/api/usuario
Authorization: Bearer <token>
Content-Type: application/json
EJEMPLO
{
  "nombre": "Maria",
  "email": "prueba@gmail.com",
  "password": "admin123",
  "rol": "User"
}
`

* Actualizar usuario
`https
PUT https://localhost:5001/api/usuario/1
Authorization: Bearer <token>
Content-Type: application/json
EJEMPLO
{
  "nombre": "Maria Actualizada",
  "rol": "Admin"
}
`

* Eliminar usuario
`http
DELETE https://localhost:5001/api/usuario/1
Authorization: Bearer <token>
`


## Auditoría – LogsController (requiere rol Admin)

* Obtener todos los logs
`https
GET https://localhost:5001/api/logs
Authorization: Bearer <token>
`

* Obtener logs por usuario
`https
GET https://localhost:5001/api/logs/usuario/1
Authorization: Bearer <token>
`

* Obtener logs por acción
`https
GET https://localhost:5001/api/logs/accion/Insertar
Authorization: Bearer <token>


