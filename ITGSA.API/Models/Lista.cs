using System;

namespace ITGSA.API.Models {
    public class Nodo<T>
    {
        public T Valor;
        public Nodo<T> Siguiente;

        public Nodo(T valor)
        {
            Valor = valor;
            Siguiente = null;
        }
    }

    public class Lista<T>
    {
        private Nodo<T> cabeza;

        public void Agregar(T valor)
        {
            Nodo<T> nuevo = new Nodo<T>(valor);

            if (cabeza == null)
            {
                cabeza = nuevo;
                return;
            }

            Nodo<T> actual = cabeza;
            while (actual.Siguiente != null)
            {
                actual = actual.Siguiente;
            }

            actual.Siguiente = nuevo;
        }

        public Nodo<T> ObtenerCabeza()
        {
            return cabeza;
        }
    }
}