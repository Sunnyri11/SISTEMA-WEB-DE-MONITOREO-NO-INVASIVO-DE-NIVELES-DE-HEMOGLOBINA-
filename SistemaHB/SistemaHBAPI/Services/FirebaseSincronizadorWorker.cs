using Firebase.Database;
using Firebase.Database.Query;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;
using SistemaHBAPI.Models;

namespace SistemaHBAPI.Services
{
    // Clase auxiliar que representa la estructura del JSON temporal que cae a Firebase
    public class DtoHemoglobinaTemporal
    {
        public int IdUsuario { get; set; }
        public int IdCiudad { get; set; }
        public int IdGenero { get; set; }
        public decimal ValorHemoglobina { get; set; }
    }

    public class FirebaseSincronizadorWorker : BackgroundService
    {
        private readonly ILogger<FirebaseSincronizadorWorker> _logger;
        private readonly IServiceProvider _serviceProvider;
        private readonly string _firebaseUrl;
        private FirebaseClient? _firebaseClient;
        private IDisposable? _firebaseSubscription;

        public FirebaseSincronizadorWorker(
            ILogger<FirebaseSincronizadorWorker> logger,
            IServiceProvider serviceProvider,
            IConfiguration configuration)
        {
            _logger = logger;
            _serviceProvider = serviceProvider;
            // Extrae la URL desde tu appsettings.json
            _firebaseUrl = configuration["FirebaseSettings:DatabaseUrl"]
                ?? "https://firebaseio.com";
        }

        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("→ Vigilante en tiempo real de Firebase inicializado correctamente.");

            _firebaseClient = new FirebaseClient(_firebaseUrl);

            // Escuchamos el nodo temporal de Firebase donde cae el JSON que se sobrescribe
            _firebaseSubscription = _firebaseClient
                .Child("analisis_temporal")
                .AsObservable<DtoHemoglobinaTemporal>()
                .Subscribe(async nodo =>
                {
                    if (nodo.Object != null && !stoppingToken.IsCancellationRequested)
                    {
                        _logger.LogInformation($"[Firebase] Datos recibidos para el Usuario ID: {nodo.Object.IdUsuario}");
                        await ProcesarAnalisisYJalarAMySQL(nodo.Object);
                    }
                });

            return Task.CompletedTask;
        }

        private async Task ProcesarAnalisisYJalarAMySQL(DtoHemoglobinaTemporal datosFirebase)
        {
            // Creamos un scope temporal para poder instanciar de forma segura tu DbContext de MySQL
            using (var scope = _serviceProvider.CreateScope())
            {
                var context = scope.ServiceProvider.GetRequiredService<SistemaHBContext>();

                try
                {
                    // ========================================================
                    // 1. PASO DE ANÁLISIS: Buscar el rango que le corresponde
                    // ========================================================
                    // Buscamos en la tabla `rango_hemoglobina` el registro que haga match con la ciudad y género
                    var rangoCorrespondiente = await context.RangoHemoglobinas
                        .FirstOrDefaultAsync(r => r.IdCiudad == datosFirebase.IdCiudad
                                               && r.IdGenero == datosFirebase.IdGenero);

                    if (rangoCorrespondiente == null)
                    {
                        _logger.LogWarning($"⚠️ No se encontró un rango de hemoglobina registrado en MySQL para Ciudad: {datosFirebase.IdCiudad} y Género: {datosFirebase.IdGenero}. Sincronización cancelada.");
                        return;
                    }

                    // LÓGICA ANALÍTICA EXTRA (Opcional): Aquí puedes evaluar si tiene anemia o está normal
                    decimal min = rangoCorrespondiente.ValorMin;
                    decimal max = rangoCorrespondiente.ValorMax;
                    decimal actual = datosFirebase.ValorHemoglobina;

                    if (actual < min)
                    {
                        _logger.LogInformation($"🔬 Análisis del Usuario {datosFirebase.IdUsuario}: Hemoglobina BAJA (Anemia).");
                    }
                    else if (actual > max)
                    {
                        _logger.LogInformation($"🔬 Análisis del Usuario {datosFirebase.IdUsuario}: Hemoglobina ALTA (Policitemia).");
                    }
                    else
                    {
                        _logger.LogInformation($"🔬 Análisis del Usuario {datosFirebase.IdUsuario}: Hemoglobina en rangos NORMALES.");
                    }

                    // ========================================================
                    // 2. PASO DE JALADO: Insertar en la tabla persistente de MySQL
                    // ========================================================
                    // Instanciamos el objeto basándonos en tu tabla `nivel_hemoglobina`
                    var nuevoNivel = new NivelHemoglobina
                    {
                        IdUsuario = datosFirebase.IdUsuario,
                        IdRangoHemoglobina = rangoCorrespondiente.IdRango, // El id_rango analizado que obtuvimos de MySQL
                        ValorHemoglobina = datosFirebase.ValorHemoglobina,
                        FechaAnalisis = DateOnly.FromDateTime(DateTime.Today)// Mapea directamente al tipo DATE de tu SQL
                    };

                    // Agregamos a la tabla de niveles y guardamos los cambios de manera asíncrona
                    await context.NivelHemoglobinas.AddAsync(nuevoNivel);
                    await context.SaveChangesAsync();

                    _logger.LogInformation($"✓ Historial de hemoglobina insertado con éxito en MySQL para el usuario {datosFirebase.IdUsuario}.");
                }
                catch (Exception ex)
                {
                    _logger.LogError($"❌ Error crítico al analizar o guardar en MySQL: {ex.Message}");
                }
            }
        }

        public override void Dispose()
        {
            _firebaseSubscription?.Dispose();
            _firebaseClient?.Dispose();
            base.Dispose();
        }
    }
}
