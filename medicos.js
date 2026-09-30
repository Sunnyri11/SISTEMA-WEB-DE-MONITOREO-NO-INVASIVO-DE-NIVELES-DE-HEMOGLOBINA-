function cerrarSesion() {
    localStorage.removeItem("token_seguridad");
    localStorage.removeItem("usuario_rol");
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
                let filaBordeColor = "";

                if (p.estadoSalud === "Anemia") {
                    conteoAnemia++;
                    filaBordeColor = "border-anemia";
                    badgeEstilo = "badge-anemia";
                } else if (p.estadoSalud === "Poliglobulia") {
                    conteoPoliglobulia++;
                    filaBordeColor = "border-poliglobulia";
                    badgeEstilo = "badge-poliglobulia";
                } else if (p.estadoSalud === "Normal") {
                    conteoNormal++;
                    filaBordeColor = "border-normal";
                    badgeEstilo = "badge-normal";
                }

                const hbValor = p.valorHemoglobina ? `${p.valorHemoglobina} g/dL` : "Sin registros";
                const letraInicial = p.nombrePaciente.charAt(0).toUpperCase();
                
                const fila = `<tr class="${filaBordeColor}">
                    <td>
                        <div class="paciente-info">
                            <div class="avatar">${letraInicial}</div>
                            <span>${p.nombrePaciente}</span>
                        </div>
                    </td>
                    <td style="text-align: center; font-weight:700;">${hbValor}</td>
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
                // Inyección del gradiente analítico cónico directamente en los estilos del elemento
                donut.style.background = `conic-gradient(
                    #10b981 0% ${finNormal}%, 
                    #ef4444 ${finNormal}% ${finAnemia}%, 
                    #f59e0b ${finAnemia}% 100%
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
