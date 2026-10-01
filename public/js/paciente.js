// Variables globales para el control del sistema
let chartHemo = null;
let datosMedicionTemporal = null; 
let historialPacienteEnMemoria = []; 
let nombrePacienteEnMemoria = "Paciente";

function cerrarSesion() {
    localStorage.removeItem("token_seguridad");
    localStorage.removeItem("usuario_rol");
    localStorage.removeItem("usuario_id");
    localStorage.removeItem("usuario_correo");
    window.location.href = "../index.html";
}

// --- CARGA INICIAL DEL EXPEDIENTE Y OPTIMIZACIÓN DE RENDIMIENTO ---
document.addEventListener("DOMContentLoaded", async function () {
    const token = localStorage.getItem("token_seguridad");
    const rol = localStorage.getItem("usuario_rol");

    const btnLogout = document.getElementById("btnCerrarSesion");
    if (btnLogout) { btnLogout.addEventListener("click", cerrarSesion); }

    if (!token || !rol || rol.trim().toLowerCase() !== "paciente") {
        alert("Acceso no autorizado. Por favor, inicie sesión nuevamente.");
        window.location.href = "../index.html";
        return;
    }

    try {
        // Consulta rápida a tu controlador Dashboard de C#
        const respuesta = await fetch(`${API_URL}Dashboard/paciente`, {
            method: "GET",
            headers: {
                "Authorization": "Bearer " + token.trim(),
                "Content-Type": "application/json"
            }
        });

        if (respuesta.ok) {
            const datos = await respuesta.json();
            
            // Guardar en memoria local para armar el PDF al instante sin volver a consultar la API
            nombrePacienteEnMemoria = datos.nombreCompleto || "Paciente";
            historialPacienteEnMemoria = datos.historial || [];

            const infoDiv = document.getElementById("infoPaciente");
            if (infoDiv) {
                infoDiv.innerHTML = `
                    <p><strong>Nombre:</strong> ${nombrePacienteEnMemoria}</p>
                    <p><strong>Correo:</strong> ${localStorage.getItem("usuario_correo") || "Registrado en el sistema"}</p>
                    <p><strong>Rol:</strong> Paciente</p>
                    <div id="estadoPaciente" class="estado">Evaluando historial...</div>
                `;
            }

            if (historialPacienteEnMemoria.length === 0) {
                const estadoDiv = document.getElementById("estadoPaciente");
                if (estadoDiv) {
                    estadoDiv.textContent = "Estado: Sin análisis registrados";
                    estadoDiv.className = "estado estable";
                }
                inicializarGrafico([], []);
                return;
            }

            // Procesar el historial de forma cronológica (De la medición más vieja a la más nueva)
            const registrosCronologicos = [...historialPacienteEnMemoria].reverse();
            
            // REQUISITO EXACTO: Convertir el eje X a un historial de "Medición 1, Medición 2..."
            const etiquetasSecuenciales = registrosCronologicos.map((r, index) => `Medición ${index + 1}`);
            const valoresHemoglobina = registrosCronologicos.map(r => parseFloat(r.valorHemoglobina || r.ValorHemoglobina || 0));

            // Evaluar el estado clínico basándose en el ÚLTIMO análisis real (posición 0 del JSON original)
            const ultimaHemoglobina = parseFloat(historialPacienteEnMemoria[0].valorHemoglobina || historialPacienteEnMemoria[0].ValorHemoglobina || 0);
            actualizarEstadoClinico(ultimaHemoglobina);
            
            // Dibujar la gráfica lineal secuencial
            inicializarGrafico(etiquetasSecuenciales, valoresHemoglobina);

        } else {
            alert("Su sesión ha expirado o es inválida.");
            cerrarSesion();
        }
    } catch (error) {
        console.error("Error al conectar con la API de pacientes:", error);
    }
});

function actualizarEstadoClinico(ultimaHemoglobina) {
    const estadoDiv = document.getElementById("estadoPaciente");
    if (!estadoDiv) return;
    if (ultimaHemoglobina < 12) {
        estadoDiv.textContent = `Estado: Alerta de Anemia (${ultimaHemoglobina} g/dL)`;
        estadoDiv.className = "estado anemia";
    } else if (ultimaHemoglobina > 17) {
        estadoDiv.textContent = `Estado: Alerta de Poliglobulia (${ultimaHemoglobina} g/dL)`;
        estadoDiv.className = "estado poliglobulia";
    } else {
        estadoDiv.textContent = `Estado: Estable (${ultimaHemoglobina} g/dL)`;
        estadoDiv.className = "estado estable";
    }
}

function inicializarGrafico(etiquetas, valores) {
    const ctxHemo = document.getElementById("graficoHemoglobina");
    if (!ctxHemo) return;
    if (chartHemo) { chartHemo.destroy(); }

    chartHemo = new Chart(ctxHemo, {
        type: "line",
        data: {
            labels: etiquetas,
            datasets: [{
                label: "Hemoglobina Registrada (g/dL)",
                data: valores,
                borderColor: "#2563eb",
                backgroundColor: "rgba(37, 99, 235, 0.15)",
                fill: true,
                tension: 0.25,
                pointRadius: 5,
                pointHoverRadius: 7
            }]
        },
        options: { 
            responsive: true, 
            maintainAspectRatio: false,
            scales: {
                y: { title: { display: true, text: 'g/dL (Gramos por decilitro)' } },
                x: { title: { display: true, text: 'Historial Secuencial Clínico' } }
            }
        }
    });
}

// ========================================================
// --- REQUISITO: GENERACIÓN DIRECTA DE REPORTE PDF ---
// ========================================================
function descargarReportePDF() {
    if (historialPacienteEnMemoria.length === 0) {
        alert("No registras análisis clínicos en tu historial para generar el reporte.");
        return;
    }

    const { jsPDF } = window.jspdf;
    const doc = new jsPDF();

    // Diseño institucional del encabezado del documento
    doc.setFillColor(15, 23, 42); 
    doc.rect(0, 0, 220, 40, "F");

    doc.setFont("helvetica", "bold");
    doc.setFontSize(22);
    doc.setTextColor(255, 255, 255);
    doc.text("SISTEMA HB - REPORTE CLÍNICO", 15, 26);

    // Información del Paciente
    doc.setFontSize(11);
    doc.setTextColor(51, 65, 85);
    doc.setFont("helvetica", "normal");
    doc.text(`Paciente: ${nombrePacienteEnMemoria}`, 15, 52);
    doc.text(`Correo Electrónico: ${localStorage.getItem("usuario_correo") || 'Registrado en el sistema'}`, 15, 58);
    doc.text(`Fecha de Emisión: ${new Date().toLocaleDateString()}`, 15, 64);
    doc.text(`Total de Análisis Procesados: ${historialPacienteEnMemoria.length}`, 15, 70);

    // Formatear filas de datos clínicos cronológicamente
    const filasTabla = [];
    const registrosOrdenTemporal = [...historialPacienteEnMemoria].reverse();
    
    registrosOrdenTemporal.forEach((r, idx) => {
        const valor = parseFloat(r.valorHemoglobina || r.ValorHemoglobina || 0);
        let diagnostico = "Normal (Estable)";
        if (valor < 12) diagnostico = "Alerta de Anemia";
        else if (valor > 17) diagnostico = "Alerta de Poliglobulia";

        filasTabla.push([
            `Medición ${idx + 1}`,
            r.fecha || r.Fecha || "Sin fecha",
            `${valor.toFixed(2)} g/dL`,
            diagnostico
        ]);
    });


    // Inyección de la tabla estructurada en el documento PDF
    doc.autoTable({
        startY: 78,
        head: [['Secuencia', 'Fecha del Análisis', 'Nivel Hemoglobina', 'Evaluación Diagnóstica']],
        body: filasTabla,
        headStyles: { fillColor: '#2980b9', fontStyle: 'bold' },
        styles: { font: 'helvetica', fontSize: 10, halign: 'center' },
        columnStyles: { 0: { halign: 'left' }, 1: { halign: 'center' } }
    });

    // Guardar el archivo directamente en las descargas del dispositivo del usuario
    const nombreArchivo = `Reporte_Hemoglobina_${nombrePacienteEnMemoria.replace(/\s+/g, '_')}.pdf`;
    doc.save(nombreArchivo);
}



// ========================================================
// --- TELEMETRÍA REAL EN VIVO DESDE LA NUBE DE FIREBASE ---
// ========================================================
function abrirModalAnalisis() {
    document.getElementById("modalAnalisis").classList.add("active");
    document.getElementById("inputSection").style.display = "block";
    document.getElementById("loaderSection").style.display = "none";
    document.getElementById("btnGuardarSQL").style.display = "none";
    document.getElementById("txtSensorId").value = "";
    document.getElementById("modalTitulo").innerText = "Vincular Dispositivo Médico";
    document.getElementById("modalMensaje").innerText = "Por favor, ingrese manualmente el código identificador de su sensor biométrico (ej. esp32_sala_1) para iniciar.";
}

function cerrarModalAnalisis() {
    document.getElementById("modalAnalisis").classList.remove("active");
    datosMedicionTemporal = null;
}

function iniciarVinculacionManual() {
    const sensorId = document.getElementById("txtSensorId").value.trim();
    if (!sensorId) {
        alert("Por favor, ingrese un código identificador válido.");
        return;
    }

    document.getElementById("inputSection").style.display = "none";
    const loader = document.getElementById("loaderSection");
    const titulo = document.getElementById("modalTitulo");
    const mensaje = document.getElementById("modalMensaje");
    const loaderTexto = document.getElementById("loaderTexto");

    loader.style.display = "flex";
    document.getElementById("iconoCarga").style.display = "block";
    titulo.innerText = "Estableciendo Enlace";
    mensaje.innerText = `Buscando canal activo para el sensor: ${sensorId}...`;

    setTimeout(() => {
        // REQUISITO EXACTO 1: Mensaje de vinculación exitosa
        loaderTexto.innerText = "Vinculación completa, porfavor utilice el dispositivo";
        mensaje.innerText = "Sincronización establecida. Realice la toma física de la muestra con el lector de hardware.";
        
        escucharCambiosFirebase(sensorId);
    }, 3000);
}

function escucharCambiosFirebase(sensorId) {
    const mensaje = document.getElementById("modalMensaje");
    const loaderTexto = document.getElementById("loaderTexto");

    // Construcción de la URL REST real hacia tu base de datos de Firebase
    const firebaseNodoUrl = `${FIREBASE_URL}analisis_temporal.json?nocache=${Date.now()}`;

    mensaje.innerText = "Esperando que el hardware envíe la telemetría biométrica...";

    // Configurar bucle de consulta activa (Polling) cada 2 segundos a Firebase
    const vigilanteIntervalo = setInterval(async () => {
        try {
            const respuestaFirebase = await fetch(firebaseNodoUrl, { method: "GET" });
            
            if (respuestaFirebase.ok) {
                const datosHardwareReal = await respuestaFirebase.json();

                // EVALUACIÓN DATOS REALES: Validar que el nodo contenga información y empareje con el ID ingresado
                if (datosHardwareReal && datosHardwareReal.sensor_id === sensorId) {
                    
                    // Detener la escucha activa de red de inmediato al capturar el evento
                    clearInterval(vigilanteIntervalo);

                    // REQUISITO EXACTO 2: Mensaje de análisis finalizado
                    loaderTexto.innerText = "Analisis terminado";

                    // Mapear de manera estricta las propiedades de tu JSON real de Firebase
                    datosMedicionTemporal = {
                        valor_hemoglobina: parseFloat(datosHardwareReal.valor_hemoglobina),
                        temperatura: parseFloat(datosHardwareReal.temperatura),
                        sensor_id: datosHardwareReal.sensor_id,
                        timestamp: datosHardwareReal.timestamp
                    };

                    // Pintar los valores REALES capturados de la nube dentro de la interfaz del modal
                    mensaje.innerHTML = `
                        <div style="text-align: left; background: #f8fafc; padding: 14px; border-radius: 10px; border: 1px solid #e2e8f0; margin-top: 10px;">
                            <p style="margin: 4px 0;"><strong>📡 Sensor validado:</strong> ${datosMedicionTemporal.sensor_id}</p>
                            <p style="margin: 4px 0; color: #2563eb;"><strong>🩸 Hemoglobina capturada:</strong> ${datosMedicionTemporal.valor_hemoglobina.toFixed(2)} g/dL</p>
                            <p style="margin: 4px 0; color: #ef4444;"><strong>🌡️ Temperatura corporal:</strong> ${datosMedicionTemporal.temperatura.toFixed(1)} °C</p>
                        </div>
                        <p style="margin-top: 15px; font-weight: 600; color: var(--text-main);">Confirme la veracidad de la muestra para guardar de manera definitiva.</p>
                    `;

                    // Habilitar el paso de confirmación manual explícito para evitar fallas
                    document.getElementById("iconoCarga").style.display = "none";
                    document.getElementById("btnGuardarSQL").style.display = "block";
                }
            }
        } catch (error) {
            console.error("Falla de comunicación con el REST de Firebase:", error);
        }
    }, 2000);

    // Cancelar la búsqueda de forma segura a los 60 segundos si el hardware no responde
    setTimeout(() => {
        if (!datosMedicionTemporal && vigilanteIntervalo) {
            clearInterval(vigilanteIntervalo);
            document.getElementById("iconoCarga").style.display = "none";
            loaderTexto.innerText = "Tiempo agotado";
            mensaje.innerText = "No se detectó el envío de datos desde el sensor. Inténtelo de nuevo.";
        }
    }, 60000);
}

// --- PASO EXTRA DE PERSISTENCIA EXPLICITA REQUERIDO ---
async function ejecutarGuardadoDefinitivo() {
    if (!datosMedicionTemporal) return;

    const token = localStorage.getItem("token_seguridad");
    const mensaje = document.getElementById("modalMensaje");
    const loaderTexto = document.getElementById("loaderTexto");
    
    document.getElementById("btnGuardarSQL").style.display = "none";
    document.getElementById("iconoCarga").style.display = "block";
    loaderTexto.innerText = "Guardando...";

    try {
        const respuestaBackend = await fetch(`${API_URL}Dashboard/guardarAnalisis`, {
            method: "POST",
            headers: {
                "Authorization": "Bearer " + token.trim(),
                "Content-Type": "application/json"
            },
            body: JSON.stringify({
                valorHemoglobina: datosMedicionTemporal.valor_hemoglobina,
                fechaAnalisis: new Date().toISOString()
            })
        });

        if (respuestaBackend.ok) {
            loaderTexto.innerText = "¡Sincronizado!";
            mensaje.innerText = "Análisis registrado de manera permanente en el servidor de la clínica.";
            setTimeout(() => {
                cerrarModalAnalisis();
                window.location.reload(); // Fuerza la recarga inmediata para volver a armar el eje X secuencial
            }, 2000);
        } else {
            alert("No se pudo completar el almacenamiento de la medición en la base de datos central.");
            document.getElementById("btnGuardarSQL").style.display = "block";
        }
    } catch (error) {
        console.error("Error al conectar con el backend de C#:", error);
        document.getElementById("btnGuardarSQL").style.display = "block";
    }
}

