using UnityEngine;

/// <summary>
/// Интерфейс для объектов, которым нужно передать внешний аудиоплеер (<see cref="AudioSource"/>)
/// </summary>
internal interface IExternalAudioPlayable
{
    AudioSource Player { get; set; }
}