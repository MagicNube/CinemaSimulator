using UnityEngine;

public interface IMaquinaReparable
{
    // Las máquinas deben tener una forma de decir si están rotas
    bool EstaRota { get; }

    // Las máquinas deben tener una función para arreglarse
    void Reparar();
}
