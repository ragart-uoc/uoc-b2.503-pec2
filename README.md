# UOC - B2.503 - PEC2

![Screenshot](pec2-screenshot.png?raw=true)

## Tanks! LAN

Este proyecto es el resultado de la segunda Práctica de Evaluación Continua (PEC2) de la asignatura Juegos Multijugador (B2.503) del Máster en Diseño y Programación de Videojuegos de la UOC.

El objetivo de la práctica era implementar una versión mejorada del tutorial Tanks! de Unity, añadiendo funcionalidades relacionadas con el multijugador en LAN e implementando componentes como Mirror.

## Características y mejoras

Se han añadido las siguientes mejoras al tutorial original:

- Soporte para hasta 4 jugadores en LAN.
- Menú principal con opciones para crear un servidor dedicado, para crear una partida nueva y para unirse a una partida existente.
- Posibilidad de cambiar el nombre y el color del tanque, tanto desde el menú principal como desde el menú de pausa.
- Indicador circular de vida para cada tanque.
- Enemigos estacionarios que deben ser destruidos para ganar la partida.
- Cámara que agrupa a todos los jugadores y enemigos en pantalla.
- Disparo alternativo con mayor velocidad y daño.
- Gestión de rondas y puntuación.
- Contador de victorias por jugador siempre visible en pantalla.
- Notificaciones de eventos en pantalla.

## Versión de Unity

El proyecto ha sido desarrollado con Unity 2021.3.18f1. La escena de inicio es `Assets/Scenes/MainMenu.unity`.

## *Builds*

No existen *builds* disponibles para este proyecto. Sin embargo, como parte de las pruebas se compiló una *build* para Windows x64 utilizando el código fuente actual que funcionó correctamente en Windows 11. 

## Cómo jugar

El juego es un *top-down shooter* en 3D. El jugador controla un tanque y debe destruir a los tanques enemigos. El jugador puede moverse y disparar dos tipos de proyectiles: uno de daño normal y otro de daño aumentado y mayor alcance. El jugador gana la ronda cuando destruye a todos los tanques enemigos. El juego finaliza cuando un jugador alcanza las 5 victorias.

Al iniciar la partida, el jugador puede elegir entre:

- Crear un servidor dedicado al que otros clientes pueden unirse.
- Crear una partida nueva en la que puede jugar y otros jugadores pueden unirse.
- Unirse a una partida existente a partir de la lista de servidores disponibles.

## Controles

Los controles que se detallan a continuación son únicamente para el modo cliente.

| Acción                         | Tecla |
|--------------------------------| --- |
| Moverse hacia delante          | Flecha arriba |
| Moverse hacia atrás            | Flecha abajo |
| Girar hacia la izquierda       | Flecha izquierda |
| Girar hacia la derecha         | Flecha derecha |
| Disparar proyectil normal      | Barra espaciadora |
| Disparar proyectil alternativo | Control izquierdo |
| Menú           | Escape |