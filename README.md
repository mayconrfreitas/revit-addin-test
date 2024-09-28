# Revit Addin Test

Code developed for the Revit Addin test, proposed by Snaptrude.

## Desafios

### Task 1: Room Data Extraction

Create a Revit add-in using **Revit API** that extracts room data from a sample Revit project (check attachment). The script should:

1. Extract room data, such as room name, number, area, and volume

2. Calculate the volume occupied by family elements such as furniture, fixtures, or equipment within each room (Exclude door/windows/voids).

3. Determine the space utilization ratio by comparing the occupied volume to the total volume of each room.

4. Categorize rooms based on their space utilization ratios using the following thresholds:

	- **Under-utilized**: Space utilization ratio less than `0.3 (30%)`
	- **Well-utilized**: Space utilization ratio between `0.3 (30%)` and `0.8 (80%)`
	- **Over-utilized**: Space utilization ratio greater than `0.8 (80%)`

5. Generate a report, either in CSV or Excel format, that lists all rooms, their total area and volume, occupied volume, space utilization ratios, and utilization categorization.


### Task 2: Import OBJ Geometry into Revit

Create a Revit add-in that imports a simple OBJ file (check attachment) containing primitive objects into a Revit project as native Revit geometry. The script should:

1. Read the provided OBJ file and parse the geometry information (vertices, faces, etc.).

2. Convert the parsed OBJ geometry data into native Revit geometry.

3. Create Revit family instances or model in-place components based on the imported geometry.

4. Place the imported geometry instances within the Revit project, maintaining their original positions and orientations from the OBJ file.

> [!CAUTION]   
> Do not use any intermediary library to parse the data.

## Solution

I started by asking about the purpose of the test, whether it would be to evaluate only the tasks themselves or also the creation of the addin, to understand if I could use a template or if I had to create it from scratch, and which version of Revit I should focus on. I was informed that I should create the addin from scratch and in any version I preferred.

I chose to create the addin for **Revit 2024**.

> [!IMPORTANT]  
> The addin was developed using the Revit API for Revit 2024.

First, I asked ChatGPT to help me configure the folder structure of my addin and plan it, following the [MVVM pattern](https://en.wikipedia.org/wiki/Model%E2%80%93view%E2%80%93viewmodel), to keep the code well-organized and as decoupled as possible.

The addin was divided into the following structure:

### Addin Structure

[📦RevitAddinText](./RevitAddinTest/) `Pasta raiz do projeto`  
 ┣ [📂AddinManifest](./RevitAddinTest/AddinManifest/) `Pasta com o manifesto do plugin`   
 ┃ ┗ [📜RevitAddinTest.addin](./RevitAddinTest/AddinManifest/RevitAddinTest.addin) `Manifesto do plugin`  
 ┣ [📂Application](./RevitAddinTest/Application/) `Arquivos referentes à aplicação`  
 ┃ ┣ [📜App.xaml](./RevitAddinTest/Application/App.xaml) `Configuração da applicação WPF do projeto`  
 ┃ ┣ [📜App.xaml.cs](./RevitAddinTest/Application/App.xaml.cs)  
 ┃ ┗ [📜RevitApp.cs](./RevitAddinTest/Application/RevitApp.cs) `Aplicação do Revit (IExternalApplication)`  
 ┣ [📂Commands](./RevitAddinTest/Commands/) `Arquivos de comando do Revit (IExternalCommand) e WPF (ICommand)`  
 ┃ ┣ 📜ImportObjGeometryCommand.cs  
 ┃ ┣ 📜RelayCommand.cs  
 ┃ ┗ 📜RoomDataExtractionCommand.cs   
 ┣ 📂Data  
 ┃ ┣ 📜Snaptrude.rvt  
 ┃ ┗ 📜snaptrude.obj  
 ┣ 📂Helpers  
 ┃ ┣ 📜FileHelper.cs  
 ┃ ┣ 📜GeometryHelper.cs  
 ┃ ┣ 📜RevitAPIHelper.cs  
 ┃ ┗ 📜WindowHelper.cs  
 ┣ 📂Models  
 ┃ ┣ 📜OBJGeometryModel.cs  
 ┃ ┗ 📜RoomModel.cs   
 ┣ 📂Properties   
 ┃ ┗ AssemblyInfo.cs   
 ┣ 📂Resources   
 ┃ ┗ 📂Icons  
 ┃   ┣ 📜import-obj-geometry-16.png  
 ┃   ┣ 📜import-obj-geometry-24.png  
 ┃   ┣ 📜room-data-extraction-16.png  
 ┃   ┗ 📜room-data-extraction-24.png  
 ┣ 📂Services  
 ┃ ┣ 📜ImportObjGeometryService.cs  
 ┃ ┣ 📜ReportService.cs  
 ┃ ┗ 📜RoomDataExtractionService.cs   
 ┣ 📂ViewModels  
 ┃ ┣ 📜BaseViewModel.cs  
 ┃ ┣ 📜ImportObjGeometryViewModel.cs   
 ┃ ┗ 📜RoomDataExtractionViewModel.cs   
 ┣ 📂Views  
 ┃ ┣ 📜ImportObjGeometryView.xaml  
 ┃ ┣ 📜ImportObjGeometryView.xaml.cs  
 ┃ ┣ 📜RoomDataExtractionView.xaml  
 ┃ ┗ 📜RoomDataExtractionView.xaml.cs   
 ┣ 📜.gitignore  
 ┣ 📜RevitAddinTest.csproj  
 ┗ 📜RevitAddinTest.sln  

Ainda sobre a estrutura geral do plugin, optei por usar os comandos do WPF sempre que possível para evitar o crash do Revit e desenvolver uma solução alinhada com o patterns escolhido. Então pedi o ChatGPT para me ajudar a criar o arquivo RelayCommand.cs, que é um arquivo que contém a implementação do ICommand do WPF de forma genérica, para que eu pudesse usar em todos os comandos do plugin.

Além disso, separei a lógica do plugin em Services, para deixar o ViewModel e os Commands mais limpos e fáceis de manter. Também criei arquivos de ajuda para códigos pontuais e repetitivos.

Para os ViwModels, criei uma classe base com a implementação do INotifyPropertyChanged, para que as demais classes pudessem herdar e não precisar repetir o código.

### Minhas Abordagens para Resolver os Problemas

Para a [Task 1](#task-1-room-data-extraction), Room Data Extraction, eu pensei em coletar todos os Rooms do projeto, pegar as informações solicitadas de cada room e fazer os cáculos necessários para determinar a utilização do espaço. Após isso, eu exibiria os dados em formato de tabela, com a possibilidade de exportar para um arquivo CSV no final.

Como não estava especificado no enunciado as unidades de medida, escolhi exibir em metros quadrados e metros cúbicos para facilitar a conferência. De toda maneira, para remover esta conversão, bastaria pegar os valores na unidade interna do Revit.

Ao coletar os Rooms e começar a extrair as informações, me deparei com alguns problemas:

1. Percebi que os Rooms no modelo estavam todos com Volume igual a 0. Pesquisei para entender e descobri que é necessário habilitar a propriedade `ComputeVolumes` da classe `AreaVolumeSettings`. Então adicionei uma verificação no início do comando para certificar que a propriedade está habilitada.

1. Mesmo após ativar a propriedade, o Volume de alguns Rooms ainda estava retornando 0. Avaliando no modelo, percebi que estes Rooms não estavam inseridos, desta maneira, considerei somente os Rooms inseridos, ou com Volume maior que 0.

1. No modelo também percebi que muitos Rooms, se não todos, estavam com o Upper Limit (level) igual ao Level e com o Limit Offset maior que o seu pé direito. Em um mundo real, eu implementaria um aviso ao usuário sobre isso, pois, aparentemente se trata de um erro de modelagem, ou consertaria automaticamente este problema, porém, para este teste, considerei que o modelo estava correto.

1. Para configurar os filtros das categorias de elementos que deveriam ser coletadas, em um mundo ideal, eu criaria um outro comando para realizar esta configuração, que consistiria em uma UI para o usuário selecionar as categorias que ele deseja coletar, porém, para este teste, criei um filtro fixo para desconsiderar as categorias mencionadas no enunciado.

	- Um dos problemas da abordagem adotada é que eu teria que criar workarounds para cada especificidade que apareça, por exemplo, na Garagem, o modelo do carro estava oculto nas vistas, porém, o volume do carro estava sendo contabilizado. Para resolver isso, eu teria que criar um filtro para desconsiderar a categoria do carro, por exemplo, ou alguma regra para considerar somente elementos visíveis no 3D.
	
	- 

1. Optei por usar o `BoundingBox` dos Rooms para criar um filtro `BoundingBoxIntersectsFilter` para coletar os elementos dentro do Room usando o `FilteredElementCollector` com o método `WherePasses()`. Porém, percebi que o filtro não estava funcionando como esperado, estava retornando elementos a mais, como tomadas e interruptores que a menor parte de sua geometria estava dentro do room, porém a maior parte estava dentro da parede. Pensei então em usar o `BoundingBoxIsInsideFilter`, porém, alguns Rooms que continham elementos simplesmente retornaram listas vazias. Acredito que tenha a ver com o ponto de inserção ou o host das famílias. Então, decidi voltar para o `BoundingBoxIntersectsFilter` e adicionar uma variável de tolerância para reduzir os limites do `BoundingBox` do Room e não coletar elementos que estavam nas paredes, pisos ou tetos.

1. Para pegar os volumes das famílias, inicialmente pensei em 

Para a [Task 2](#task-2-import-obj-geometry-into-revit), Import OBJ Geometry into Revit, 


> [!IMPORTANT]  
> Para mais detalhes e maiores informações, verificar os comentários no código!