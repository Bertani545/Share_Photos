using SQLite;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using SharePhotos.Models;

namespace SharePhotos
{
    public class Repository
    {
        private readonly string _path; //Se acostumbra poner _ para diferenicar una variable privada

        public string StatusMessage { get; set; } //Para verficiar si se realizó correctamentw

        private SQLiteConnection conn;

        private void Init()
        {
            StatusMessage = "";
            //Verificar si ya tenemos los archivos y tablas hechas o no
            if (conn is not null)
            {
                return;
            }
            else
            {
                //Si no hay conección creamos una nueva con la ruta
                conn = new(_path);
            }
            conn.CreateTable<Photo>();
        }

        public Repository(string path)
        {
            this._path = path;
        }

        //Creamos registros
        public void AgregarRegistro(Photo photo)
        {
            try
            {
                Init(); //Revisa si hay tabla o la crea
                if (photo == null)
                {
                    throw new Exception("Error al guardar");
                }
                else
                {
                    Photo photoGuardar = new Photo(); //lo instanciamos para no editar el original
                    photoGuardar = photo;

                    //Insertamos los datos
                    int resultado = conn.Insert(photoGuardar);
                    StatusMessage = "Registro Guardado: " + resultado;
                }
            }
            catch (Exception ex)
            {
                StatusMessage = "Error al guardar";
            }
        }

        public List<Photo> ObtenerRegistros()
        {
            try
            {
                Init();
                StatusMessage = "Datos obtenidos";
                return conn.Table<Photo>().ToList();
            }
            catch (Exception ex)
            {
                StatusMessage = "Error al obtener datos. " + ex.Message;
            }
            StatusMessage = "Default";
            return new List<Photo>();
        }
        public string get_status()
        {
            return StatusMessage;
        }
    }
}
