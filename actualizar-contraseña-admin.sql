-- Script para actualizar la contraseña del administrador
-- Ejecutar este script en la base de datos si es necesario

-- Primero, verificar si existe el usuario admin
SELECT Id, Nombre, Correo, Rol FROM Estudiantes WHERE Correo = 'admin@admin.com';

-- Actualizar la contraseña del admin (hash de SHA256 de 'Contraseña2025*')
-- El hash de 'Contraseña2025*' es: 'Q29udHJhc2XDsWEyMDI1Kg=='
UPDATE Estudiantes 
SET PasswordHash = 'Q29udHJhc2XDsWEyMDI1Kg==' 
WHERE Correo = 'admin@admin.com';

-- Verificar que se actualizó correctamente
SELECT Id, Nombre, Correo, Rol FROM Estudiantes WHERE Correo = 'admin@admin.com';

-- Nota: Si el usuario admin no existe, se creará automáticamente al ejecutar la aplicación
-- con la nueva contraseña configurada en el DataSeeder 