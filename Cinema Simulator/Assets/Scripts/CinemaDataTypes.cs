using UnityEngine;
using System;

// 1. Definimos los Géneros disponibles (basado en tu tabla)
public enum CinemaGenre
{
    Familiar,
    Animacion,
    Terror,
    Drama,
    Romance,
    Deportes,
    Aventura,
    CienciaFiccion,
    Accion,
    Musical,
    Misterio,
    Fantasia,
    Comedia
}

// 2. Estructura para una "Noticia Diaria" (Una fila de tu tabla)
[Serializable]
public struct NewsScenario
{
    [TextArea(3, 5)] // Hace el campo de texto más grande en el inspector
    public string headline; // La frase del periódico
    public CinemaGenre correctGenre;
    public CinemaGenre neutralGenre;
    public CinemaGenre incorrectGenre;
}

// 3. Estructura para una "Película" (Tus assets)
[Serializable]
public struct MovieAsset
{
    public string title;
    public Sprite posterImage; // La imagen para el botón de la tablet
    public CinemaGenre genre; // El género de esta película
}