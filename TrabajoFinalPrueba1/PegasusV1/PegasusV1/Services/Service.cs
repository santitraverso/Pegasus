﻿using PegasusV1.Entities;
using PegasusV1.Interfaces;
using System.Linq.Dynamic.Core;
using System.Linq.Expressions;

namespace PegasusV1.Services
{
    public class Service<T> : IService<T> where T : class
    {
        protected readonly IRepository<T> Repository;

        public Service(IRepository<T> repository)
        {
            Repository = repository;
        }

        public async Task<T?> GetById(int id, Expression<Func<T, object>>[]? includes = null)
        {
            string query = $"Id == {id}";
            var p = Expression.Parameter(typeof(T), query);
            var e = (Expression)DynamicExpressionParser.ParseLambda(new[] { p }, null, query);
            var ex = (Expression<Func<T, bool>>)e;

            return await Repository.GetById(ex);
        }

        public async Task<List<T>> GetForCombo(Expression<Func<T, bool>>? predicate = null, Expression<Func<T, object>>[]? includes = null)
        {
            return await Repository.GetForCombo(predicate, includes);
        }

        public async Task<T> Create(T entity)
        {
            return await Repository.Create(entity);
        }

        public async Task<T> Update(T entity)
        {
            return await Repository.Update(entity);
        }

        public async Task Delete(T entity)
        {
            await Repository.Delete(entity);
        }
        
        public async Task<List<T>> CreateAll(List<T> entities)
        {
            return await Repository.CreateAll(entities);
        }

        public async Task<List<T>> UpdateAll(List<T> entities)
        {
            return await Repository.UpdateAll(entities);
        }

        public async Task DeleteAll(List<T> entities)
        {
            await Repository.DeleteAll(entities);
        }

        public async Task<List<CursoMateria>> GetCursoMateriaForCombo(Expression<Func<CursoMateria, bool>>? predicate = null)
        {
            return await Repository.GetCursoMateriaForCombo(predicate);
        }

        public async Task<List<DocenteMateria>> GetDocenteMateriaForCombo(Expression<Func<DocenteMateria, bool>>? predicate = null)
        {
            return await Repository.GetDocenteMateriaForCombo(predicate);
        }

        public async Task<List<IntegrantesEventos>> GetIntegrantesEventosForCombo(Expression<Func<IntegrantesEventos, bool>>? predicate = null)
        {
            return await Repository.GetIntegrantesEventosForCombo(predicate);
        }

        public async Task<List<Asistencia>> GetAsistenciasForCombo(Expression<Func<Asistencia, bool>>? predicate = null)
        {
            return await Repository.GetAsistenciasForCombo(predicate);
        }

        public async Task<List<CuadernoComunicados>> GetCuadernoComunicadosForCombo(Expression<Func<CuadernoComunicados, bool>>? predicate = null)
        {
            return await Repository.GetCuadernoComunicadosForCombo(predicate);
        }
        
        public async Task<List<Desempenio>> GetDesempenoForCombo(Expression<Func<Desempenio, bool>>? predicate = null)
        {
            return await Repository.GetDesempenoForCombo(predicate);
        }

        public async Task<List<DesempenioAlumnos>> GetDesempenioAlumnosForCombo(Expression<Func<DesempenioAlumnos, bool>>? predicate = null)
        {
            return await Repository.GetDesempenioAlumnosForCombo(predicate);
        }

        public async Task<List<ComunicadoAlumnos>> GetComunicadoAlumnosForCombo(Expression<Func<ComunicadoAlumnos, bool>>? predicate = null)
        {
            return await Repository.GetComunicadoAlumnosForCombo(predicate);
        }

        public async Task<List<Hijo>> GetHijoForCombo(Expression<Func<Hijo, bool>>? predicate = null)
        {
            return await Repository.GetHijoForCombo(predicate);
        }

        public async Task<List<Pago>> GetPagoForCombo(Expression<Func<Pago, bool>>? predicate = null)
        {
            return await Repository.GetPagoForCombo(predicate);
        }

        public async Task<List<Tarea>> GetTareaForCombo(Expression<Func<Tarea, bool>>? predicate = null)
        {
            return await Repository.GetTareaForCombo(predicate);
        }
        
        public async Task<List<Contenido>> GetContenidoForCombo(Expression<Func<Contenido, bool>>? predicate = null)
        {
            return await Repository.GetContenidoForCombo(predicate);
        }
        public async Task<List<Curso>> GetCursoForCombo(Expression<Func<Curso, bool>>? predicate = null)
        {
            return await Repository.GetCursoForCombo(predicate);
        }

        public async Task<List<IntegrantesCursos>> GetIntegrantesCursosForCombo(Expression<Func<IntegrantesCursos, bool>>? predicate = null)
        {
            return await Repository.GetIntegrantesCursosForCombo(predicate);
        }

        public async Task<List<Calificaciones>> GetCalificacionesForCombo(Expression<Func<Calificaciones, bool>>? predicate = null)
        {
            return await Repository.GetCalificacionesForCombo(predicate);
        }

        public async Task<List<Modulos>> GetModulosForCombo(Expression<Func<Modulos, bool>>? predicate = null)
        {
            return await Repository.GetModulosForCombo(predicate);
        }

        public async Task<List<ModulosPerfiles>> GetModulosPerfilesForCombo(Expression<Func<ModulosPerfiles, bool>>? predicate = null)
        {
            return await Repository.GetModulosPerfilesForCombo(predicate);
        }

        public async Task<List<Perfiles>> GetPerfilesForCombo(Expression<Func<Perfiles, bool>>? predicate = null)
        {
            return await Repository.GetPerfilesForCombo(predicate);
        }

        public async Task<List<Calificaciones>> GetCalificacionesByUser(int userId)
        {
            return await Repository.GetCalificacionesByUser(userId);
        }

        public async Task<List<Calificaciones>> GetCalificacionesByMateria(int materiaId)
        {
            return await Repository.GetCalificacionesByMateria(materiaId);
        }

        public async Task<List<Calificaciones>> GetCalificacionesByUserAndMateria(int userId, int materiaId)
        {
            return await Repository.GetCalificacionesByUserAndMateria(userId, materiaId);
        }
        
        public async Task<List<ContenidoMaterias>> GetContenidoMateriasForCombo(Expression<Func<ContenidoMaterias, bool>>? predicate = null)
        {
            return await Repository.GetContenidoMateriasForCombo(predicate);
        }
    }
}
