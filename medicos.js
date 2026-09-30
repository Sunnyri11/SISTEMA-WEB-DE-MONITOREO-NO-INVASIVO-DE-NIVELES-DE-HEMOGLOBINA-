function cerrarSesion() {
    localStorage.removeItem("token_seguridad");
    localStorage.removeItem("usuario_role");
    localStorage.removeItem("usuario_id");
    window.location.href = "index.html";
}

document.addEventListener("DOMContentLoaded", async function () {
    const token = localStorage.getItem("token_seguridad");
    const rol = localStorage.getItem("usuario_rol");

    const btnLogout = document.getElementById("btnCerrarSesion");
    if (btnLogout) {
        btnLogout.addEventListener("click", cerrarSesion);
    }

    if (!token || !rol || rol.trim().toLowerCase() !== "medico") {
        alert("Acceso no autorizado. Inicie sesión nuevamente.");
        window.location.href = "index.html";
        return;
    }

    try {
        const respuesta = await fetch(`${API_URL}Dashboard/medico`, {
            method: "GET",
            headers: {
                "Authorization": "Bearer " + token.trim(),
                "Content-Type": "application/json"
            }
        });

        if (respuesta.ok) {
            const datos = await respuesta.json();
            
            document.getElementById("txtMedico").innerText = datos.nombreMedico;

            const cuerpoTabla = document.getElementById("listaPacientes");
            cuerpoTabla.innerHTML = "";

            let conteoAnemia = 0;
            let conteoPoliglobulia = 0;
            let conteoNormal = 0;

            if (!datos.pacientes || datos.pacientes.length === 0) {
                document.getElementById("lblTotalPacientes").innerText = "0";
                document.getElementById("graficoDonut").style.display = "none";
                document.getElementById("mensajeSinPacientes").style.display = "block";
                cuerpoTabla.innerHTML = `<tr><td colspan="3" style="text-align:center; color:#64748b; padding:20px;">No tiene pacientes asignados.</td></tr>`;
                return;
            }

            document.getElementById("lblTotalPacientes").innerText = datos.pacientes.length;

            datos.pacientes.forEach(p => {
                let badgeEstilo = "";
                let colorAvatar = ""; // Nueva variable para controlar el color del círculo del avatar

                if (p.estadoSalud === "Anemia") {
                    conteoAnemia++;
                    badgeEstilo = "badge-anemia";
                    colorAvatar = "var(--color-anemia)";
                } else if (p.estadoSalud === "Poliglobulia") {
                    conteoPoliglobulia++;
                    badgeEstilo = "badge-poliglobulia";
                    colorAvatar = "var(--color-poliglobulia)";
                } else if (p.estadoSalud === "Normal") {
                    conteoNormal++;
                    badgeEstilo = "badge-normal";
                    colorAvatar = "var(--color-normal)";
                }

                const hbValor = p.valorHemoglobina ? `${p.valorHemoglobina} g/dL` : "Sin registros";
                const letraInicial = p.nombrePaciente.charAt(0).toUpperCase();
                
                // Se reemplazó .paciente-info por .patient-cell para emparejar con el CSS unificado
                const fila = `<tr title="Pasar el mouse para inspeccionar el historial clínico de ${p.nombrePaciente}">
                    <td>
                        <div class="patient-cell">
                            <div class="avatar" style="background-color: ${colorAvatar};">${letraInicial}</div>
                            <span>${p.nombrePaciente}</span>
                        </div>
                    </td>
                    <td style="text-align: center;" class="hb-value">${hbValor}</td>
                    <td style="text-align: center;"><span class="badge ${badgeEstilo}">${p.estadoSalud}</span></td>
                </tr>`;
                cuerpoTabla.innerHTML += fila;
            });

            // 📊 Cálculo matemático del gráfico de dona nativo (CSS puro)
            const total = conteoNormal + conteoAnemia + conteoPoliglobulia;
            if (total > 0) {
                const porcNormal = (conteoNormal / total) * 100;
                const porcAnemia = (conteoAnemia / total) * 100;
                
                const finNormal = porcNormal;
                const finAnemia = finNormal + porcAnemia;

                const donut = document.getElementById("graficoDonut");
                
                // Se actualiza el Tooltip interactivo con los valores en tiempo real al pasar el mouse por encima
                donut.title = `Distribución actual:\n• Normales: ${conteoNormal}\n• Anemia: ${conteoAnemia}\n• Poliglobulia: ${conteoPoliglobulia}`;
                
                // Inyección del gradiente analítico cónico directamente en los estilos del elemento
                donut.style.background = `conic-gradient(
                    var(--color-normal) 0% ${finNormal}%, 
                    var(--color-anemia) ${finNormal}% ${finAnemia}%, 
                    var(--color-poliglobulia) ${finAnemia}% 100%
                )`;
            }

        } else {
            alert("Sesión no válida o expirada.");
            cerrarSesion();
        }
    } catch (error) {
        console.error("Error crítico de comunicación de red:", error);
    }
});
