# <center>Proyecto 3Models-SRI</center>

![portada](img/portada.jpeg)

Integrantes:

* Leonardo Ulloa Ferrer

* Tony Cadahía Poveda

* Alejandra Monzón Peña

Descripción del Problema:

Este proyecto tiene como objetivo implementar varios modelos de recuperación de información para su posterior análisis sobre distintas colecciones de documentos.

**General**:

En esta pre-entrega se implementó el modelo vectorial y la interfaz gráfica que utilizará el usuario. Con el objetivo de que el proyecto sea escalable y reusable se dividió el procesamiento de los documentos en varios componentes:

* Lectura del Documento: La obtención de los caracteres de los documentos
  
* Procesar el documento: La transformación de un documento a una tabla de términos y frecuencias

* Construcción de la estructura para el modelo: Utilizar la información de las tablas de términos y frecuencias de cada documento para construir una estructura que permita realizar consultas eficientemente

* Función de Ranking: Utilización de la información almacenada en la estructura para el modelo para obtener una evaluación de la similitud

**Lectura**:

Para esta componente implementamos un tipo para encapsular el concepto de documento, el cual lo interpreta como un iterador de caracteres. Para evitar el consumo innecesario de memoria la lectura del documento se realiza de forma perezosa (lazy). Además, recibe una función para obtener metadatos del documento que va a estar en correspondencia con la colección de la que procede, dichos metadatos también se obtienen en demanda. De esto se encarga la clase Document.

**Procesamiento del documento**:

Para construir la tabla de términos y frecuencias contamos la ocurrencia de cada uno de los términos utilizando la estructura comentada en la sección anterior y un diccionario. Solo se tienen en cuenta los documentos que no pertenecen a un conjunto de Stopwords que tenemos almacenados en un hashset.

**Construcción del modelo**:

Para que la consulta pueda efectuarse de forma eficiente se realiza un preprocesado de la colección en cuestión y la información obtenida es almacenada en una estructura de datos optimizada para procesar consultas de un modelo determinado. Para el modelo vectorial se usó un diccionario de términos-documentos encapsulado en la implementación de un IStorage del subproyecto SRI. Utilizamos el diccionario de C# que utiliza un hashset para almacenar los pares \<llave,valor> y una linked list para almacenar los documentos asociados a cada término. Para la construcción de esta estructura solo se necesita un enumerable de documentos, los cuales se van incorporando a la misma de uno en uno, en un proceso que consiste en transformar el documento en tabla de frecuencia e utilizar la información de la misma para actualizar la estructura, de esta forma en ningún momento de la construcción se tiene almacenado en memoria más de una tabla de términos y frecuencia.

**Similitud y Ranking**:

Decidimos encapsular el concepto de semejanza y recuperación de documentos en la interfaz ISRIModel para separar el preprocesado y la estructura que resulta de él, del procesamiento de la consulta, para permitir que más de un modelo reutilice la misma estructura.

**Interfaz Gráfica**:

Para la interfaz gráfica utilizamos la tecnología Razor Pages y consiste en un textbox para escribir la consulta y un botón para efectuarla; al efectuarla se listan los documentos en orden decreciente de la similitud con la consulta. En el componente de la interfaz gráfica está el código que unifica las componentes anteriores, obtiene los documentos de las direcciones especificadas, ordena el preprocesado de los mismos y cuando se efectúa una consulta interactúa con el ISRIModel parar obtener la lista de documentos con su respectivo índice de similitud, los ordena en función de este valor y los muestra utilizando el título y una breve sección de texto obtenido de los metadatos como identificador de cada uno.
