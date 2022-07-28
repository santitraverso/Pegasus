set transaction read write; 

INSERT INTO  EVENTOS (NOMBRE, DESCRIPCION, FECHA) VALUES (N'Acto de inicio de año 2022', N'Convocatoria para padres y alumnos para conmemorar el inicio del año escolar.', CAST(N'2022-03-14T09:00:00.000' AS TIMESTAMP(3)));
INSERT INTO  EVENTOS (NOMBRE, DESCRIPCION, FECHA) VALUES (N'Comunión', N'Se celebra la comunión de los alumnos de 4° año.', CAST(N'2022-05-19T12:00:00.000' AS TIMESTAMP(3)));

INSERT INTO  USUARIOS (ACTIVO, NOMBRE, APELLIDO, PERFIL, MAIL) VALUES (true, N'Rodrigo', N'Santalla', N'ALUMNO', N'rodrigo.santalla@davinci.edu.ar');
INSERT INTO  USUARIOS (ACTIVO, NOMBRE, APELLIDO, PERFIL, MAIL) VALUES (true, N'Santiago', N'Traverso', N'DOCENTE', N'santiago.traverso@davinci.edu.ar');
INSERT INTO  USUARIOS (ACTIVO, NOMBRE, APELLIDO, PERFIL, MAIL) VALUES (true, N'Santiago', N'Linares', N'PRECEPTOR', N'santiago.linares@davinci.edu.ar');
INSERT INTO  USUARIOS (ACTIVO, NOMBRE, APELLIDO, PERFIL, MAIL) VALUES (true, N'Lucas', N'Castro', N'PADRE', N'lucas.castro45@davinci.edu.ar');
INSERT INTO  USUARIOS (ACTIVO, NOMBRE, APELLIDO, PERFIL, MAIL) VALUES (true, N'PEGASUS_2', N'Company', N'ADMIN', N'PEGASUS_2.company@davinci.edu.ar');

INSERT INTO  INTEGRANTES_EVENTOS (ID_EVENTO, ID_USUARIO) VALUES (2, 21);
INSERT INTO  INTEGRANTES_EVENTOS (ID_EVENTO, ID_USUARIO) VALUES (2, 22);
INSERT INTO  INTEGRANTES_EVENTOS (ID_EVENTO, ID_USUARIO) VALUES (2, 24);
INSERT INTO  INTEGRANTES_EVENTOS (ID_EVENTO, ID_USUARIO) VALUES (2, 25);
INSERT INTO  INTEGRANTES_EVENTOS (ID_EVENTO, ID_USUARIO) VALUES (3, 21);
INSERT INTO  INTEGRANTES_EVENTOS (ID_EVENTO, ID_USUARIO) VALUES (3, 23);
INSERT INTO  INTEGRANTES_EVENTOS (ID_EVENTO, ID_USUARIO) VALUES (3, 25);

INSERT INTO  MATERIAS (NOMBRE, CURSO) VALUES (N'Matemática', N'1°A');
INSERT INTO  MATERIAS (NOMBRE, CURSO) VALUES (N'Matemática', N'1°B');
INSERT INTO  MATERIAS (NOMBRE, CURSO) VALUES (N'Matemática', N'2°A');
INSERT INTO  MATERIAS (NOMBRE, CURSO) VALUES (N'Matemática', N'2°B');
INSERT INTO  MATERIAS (NOMBRE, CURSO) VALUES (N'Lengua', N'1°A');
INSERT INTO  MATERIAS (NOMBRE, CURSO) VALUES (N'Lengua', N'2°A');
INSERT INTO  MATERIAS (NOMBRE, CURSO) VALUES (N'Inglés', N'1°A');
INSERT INTO  MATERIAS (NOMBRE, CURSO) VALUES (N'Inglés', N'1°B');

INSERT INTO  ASISTENCIA (ID_ALUMNO, ID_MATERIA, FECHA, PRESENTE) VALUES (21, 1, CAST(N'2022-03-14T00:00:00.000' AS TIMESTAMP(3)), true);
INSERT INTO  ASISTENCIA (ID_ALUMNO, ID_MATERIA, FECHA, PRESENTE) VALUES (21, 1, CAST(N'2022-03-15T00:00:00.000' AS TIMESTAMP(3)), true);
INSERT INTO  ASISTENCIA (ID_ALUMNO, ID_MATERIA, FECHA, PRESENTE) VALUES (21, 5, CAST(N'2022-03-14T00:00:00.000' AS TIMESTAMP(3)), true);
INSERT INTO  ASISTENCIA (ID_ALUMNO, ID_MATERIA, FECHA, PRESENTE) VALUES (21, 5, CAST(N'2022-03-17T00:00:00.000' AS TIMESTAMP(3)), false);

INSERT INTO  INTEGRANTES_MATERIAS (ID_MATERIA, ID_USUARIO) VALUES (1, 21);
INSERT INTO  INTEGRANTES_MATERIAS (ID_MATERIA, ID_USUARIO) VALUES (1, 23);
INSERT INTO  INTEGRANTES_MATERIAS (ID_MATERIA, ID_USUARIO) VALUES (5, 21);
INSERT INTO  INTEGRANTES_MATERIAS (ID_MATERIA, ID_USUARIO) VALUES (7, 21);


INSERT INTO  TAREAS (TITULO, DESCRIPCION, FECHA_ENTREGA, ENTREGADO, CALIFICACION, ID_MATERIA, ID_ALUMNO) VALUES (N'Ejercicios 1-5', N'Completar los ejercicios 1-5 de la unidad 2 utilizando los métodos vistos en clase', CAST(N'2022-02-01T00:00:00.000' AS TIMESTAMP(3)), false, 8.5, 1, 21);
INSERT INTO  TAREAS (TITULO, DESCRIPCION, FECHA_ENTREGA, ENTREGADO, CALIFICACION, ID_MATERIA, ID_ALUMNO) VALUES (N'Evaluación Polinomios', N'', CAST(N'2022-02-25T00:00:00.000' AS TIMESTAMP(3)), true, 7, 1, 21);
INSERT INTO  TAREAS (TITULO, DESCRIPCION, FECHA_ENTREGA, ENTREGADO, CALIFICACION, ID_MATERIA, ID_ALUMNO) VALUES (N'Evaluación Análisis sintáctico', N'', CAST(N'2022-03-07T00:00:00.000' AS TIMESTAMP(3)), true, 8, 5, 21);
INSERT INTO  TAREAS (TITULO, DESCRIPCION, FECHA_ENTREGA, ENTREGADO, CALIFICACION, ID_MATERIA, ID_ALUMNO) VALUES (N'Informe sobre los tipos de poesía', N'Desarrollar un informe de por 500-750 palabras sobre los tipos de poesía vistos en clase', CAST(N'2022-03-12T00:00:00.000' AS TIMESTAMP(3)), false, 0, 5, 21);


INSERT INTO  CONTENIDO (ID_MATERIA, UNIDAD, TITULO, DESCRIPCION) VALUES (1, 1, N'Polinomios', N'Expresión algebraica formada por varios términos, en los cuales intervienen números y letras relacionados dentro de operaciones.');
INSERT INTO  CONTENIDO (ID_MATERIA, UNIDAD, TITULO, DESCRIPCION) VALUES (1, 2, N'Factorización', N'Descomposición en factores de una expresión algebraica en forma de producto.');
INSERT INTO  CONTENIDO (ID_MATERIA, UNIDAD, TITULO, DESCRIPCION) VALUES (2, 1, N'Polinomios', N'Expresión algebraica formada por varios términos, en los cuales intervienen números y letras relacionados dentro de operaciones.');
INSERT INTO  CONTENIDO (ID_MATERIA, UNIDAD, TITULO, DESCRIPCION) VALUES (2, 2, N'Factorización', N'Descomposición en factores de una expresión algebraica en forma de producto.');
INSERT INTO  CONTENIDO (ID_MATERIA, UNIDAD, TITULO, DESCRIPCION) VALUES (3, 1, N'Derivadas', N'Razón de cambio instantánea con la que varía el valor de una función matemática.');
INSERT INTO  CONTENIDO (ID_MATERIA, UNIDAD, TITULO, DESCRIPCION) VALUES (3, 1, N'Integrales', N'Generalización de la suma de infinitos sumandos, infinitesimalmente pequeños.');
INSERT INTO  CONTENIDO (ID_MATERIA, UNIDAD, TITULO, DESCRIPCION) VALUES (5, 1, N'Análisis sintáctico', N'Análisis de las funciones sintácticas o relaciones de concordancia y jerarquía que guardan las palabras cuando se agrupan entre sí.');
INSERT INTO  CONTENIDO (ID_MATERIA, UNIDAD, TITULO, DESCRIPCION) VALUES (5, 1, N'Poesía', N'Género literario considerado como una manifestación de la belleza o del sentimiento estético por medio de la palabra, en verso o en prosa.');
INSERT INTO  CONTENIDO (ID_MATERIA, UNIDAD, TITULO, DESCRIPCION) VALUES (7, 1, N'Past simple', N'Tiempo verbal que se usa en el idioma inglés para narrar hechos que ocurrieron en un momento específico del pasado.');
INSERT INTO  CONTENIDO (ID_MATERIA, UNIDAD, TITULO, DESCRIPCION) VALUES (7, 1, N'Present simple', N'Tiempo verbal que se usa en el idioma inglés para narrar hechos que ocurren en el momento presente.');

INSERT INTO  CUADERNO_COMUNICADOS (ID_ALUMNO, ID_PROFESOR, DESCRIPCION, FECHA) VALUES (21, 23, N'El alumno no cumplió con el código de vestimenta impuesto por la institución.', CAST(N'2022-05-15T12:00:00.000' AS TIMESTAMP(3)));
INSERT INTO  CUADERNO_COMUNICADOS (ID_ALUMNO, ID_PROFESOR, DESCRIPCION, FECHA) VALUES (21, 23, N'El alumno recibe un llamado de atención por usar el teléfono en clase', CAST(N'2022-05-12T12:00:00.000' AS TIMESTAMP(3)));

INSERT INTO  DESEMPEÑO (ID_ALUMNO, DESCRIPCION, FECHA) VALUES (21, N'El desempeño del alumno es satisfactorio, demuestra esfuerzo y dedicación en sus trabajos', CAST(N'2022-04-10T12:00:00.000' AS TIMESTAMP(3)));

INSERT INTO  HIJOS (ID_PADRE, ID_HIJO) VALUES (25, 21);

INSERT INTO  PAGOS (ID_ALUMNO, CONCEPTO, MONTO, PAGADO, VENCIMIENTO) VALUES (21, N'Cuota Enero 2022', 15200, true, CAST(N'2022-02-05T00:00:00.000' AS TIMESTAMP(3)));
INSERT INTO  PAGOS (ID_ALUMNO, CONCEPTO, MONTO, PAGADO, VENCIMIENTO) VALUES (21, N'Cuota Febrero 2022', 15200, true, CAST(N'2022-03-05T00:00:00.000' AS TIMESTAMP(3)));
INSERT INTO  PAGOS (ID_ALUMNO, CONCEPTO, MONTO, PAGADO, VENCIMIENTO) VALUES (21, N'Cuota Marzo 2022', 16400, true, CAST(N'2022-04-05T00:00:00.000' AS TIMESTAMP(3)));
INSERT INTO  PAGOS (ID_ALUMNO, CONCEPTO, MONTO, PAGADO, VENCIMIENTO) VALUES (21, N'Cuota Abril 2022', 16400, true, CAST(N'2022-05-05T00:00:00.000' AS TIMESTAMP(3)));
INSERT INTO  PAGOS (ID_ALUMNO, CONCEPTO, MONTO, PAGADO, VENCIMIENTO) VALUES (21, N'Cuota Enero 2022', 16400, false, CAST(N'2022-06-05T00:00:00.000' AS TIMESTAMP(3)));

INSERT INTO  CONTACTOS (NOMBRE, MAIL, TELEFONO) VALUES (N'Tesorería', N'tesoreria@davinci.edu.ar', N'15-2222-2222');
INSERT INTO  CONTACTOS (NOMBRE, MAIL, TELEFONO) VALUES (N'Escuela Da Vinci', N'davinci@davinci.edu.ar', N'15-1111-1111');
INSERT INTO  CONTACTOS (NOMBRE, MAIL, TELEFONO) VALUES (N'Dpto. Alumnos', N'alumnos@davinci.edu.ar', N'15-0000-0000');

