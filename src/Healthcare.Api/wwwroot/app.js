const state = {
    token: localStorage.getItem("healthcare_demo_token") || "",
    currentUser: null,
    patients: [],
    appointments: [],
    prescriptions: [],
    patientLookup: {},
    doctorLookup: {}
};

const elements = {
    statusPanel: document.getElementById("status-panel"),
    authState: document.getElementById("auth-state"),
    currentUser: document.getElementById("current-user"),
    loginForm: document.getElementById("login-form"),
    logoutButton: document.getElementById("logout-button"),
    patientsForm: document.getElementById("patients-form"),
    patientEditorForm: document.getElementById("patient-editor-form"),
    patientFormTitle: document.getElementById("patient-form-title"),
    appointmentsForm: document.getElementById("appointments-form"),
    appointmentEditorForm: document.getElementById("appointment-editor-form"),
    appointmentFormTitle: document.getElementById("appointment-form-title"),
    appointmentSelection: document.getElementById("appointment-selection"),
    prescriptionsForm: document.getElementById("prescriptions-form"),
    prescriptionEditorForm: document.getElementById("prescription-editor-form"),
    prescriptionFormTitle: document.getElementById("prescription-form-title"),
    prescriptionSelection: document.getElementById("prescription-selection"),
    medicinesEditor: document.getElementById("medicines-editor"),
    historyForm: document.getElementById("history-form"),
    historyPanel: document.getElementById("history-panel"),
    auditForm: document.getElementById("audit-form"),
    patientsBody: document.getElementById("patients-body"),
    appointmentsBody: document.getElementById("appointments-body"),
    prescriptionsList: document.getElementById("prescriptions-list"),
    auditBody: document.getElementById("audit-body")
};

elements.loginForm.addEventListener("submit", handleLogin);
elements.logoutButton.addEventListener("click", logout);
document.getElementById("load-patients").addEventListener("click", loadPatients);
document.getElementById("load-appointments").addEventListener("click", loadAppointments);
document.getElementById("load-prescriptions").addEventListener("click", loadPrescriptions);
document.getElementById("save-prescription").addEventListener("click", savePrescription);
document.getElementById("prescription-reset").addEventListener("click", resetPrescriptionEditor);
document.getElementById("add-medicine").addEventListener("click", () => addMedicineRow());
document.getElementById("load-history").addEventListener("click", loadPatientHistory);
document.getElementById("load-audit").addEventListener("click", loadAuditLogs);
document.getElementById("save-patient").addEventListener("click", savePatient);
document.getElementById("patient-reset").addEventListener("click", resetPatientEditor);
document.getElementById("save-appointment").addEventListener("click", saveAppointment);
document.getElementById("update-appointment-status").addEventListener("click", updateAppointmentStatus);
document.getElementById("appointment-reset").addEventListener("click", resetAppointmentEditor);
elements.patientsBody.addEventListener("click", handlePatientTableClick);
elements.appointmentsBody.addEventListener("click", handleAppointmentTableClick);
elements.prescriptionsList.addEventListener("click", handlePrescriptionListClick);

initialize();

async function initialize() {
    if (!state.token) {
        renderAuthState();
        return;
    }

    try {
        const response = await apiFetch("/api/v1/auth/me");
        state.currentUser = response.data;
        setStatus(`Welcome back, ${state.currentUser.full_name}.`, "success");
    } catch (error) {
        logout(false);
        setStatus(error.message, "error");
    }

    renderAuthState();
}

async function handleLogin(event) {
    event.preventDefault();
    const formData = new FormData(elements.loginForm);

    try {
        const response = await apiFetch("/api/v1/auth/login", {
            method: "POST",
            headers: {
                "Content-Type": "application/json"
            },
            body: JSON.stringify({
                username: formData.get("username"),
                password: formData.get("password")
            })
        }, false);

        state.token = response.data.access_token;
        state.currentUser = response.data.user;
        localStorage.setItem("healthcare_demo_token", state.token);
        renderAuthState();
        setStatus(`Signed in as ${state.currentUser.full_name}.`, "success");
        await Promise.all([loadPatients(), loadAppointments(), loadPrescriptions(), loadAuditLogs()]);
    } catch (error) {
        setStatus(error.message, "error");
    }
}

function logout(showMessage = true) {
    state.token = "";
    state.currentUser = null;
    localStorage.removeItem("healthcare_demo_token");
    renderAuthState();

    if (showMessage) {
        setStatus("You have been signed out.", "success");
    }
}

function renderAuthState() {
    const isAuthenticated = Boolean(state.token && state.currentUser);
    elements.authState.textContent = isAuthenticated ? `Signed in as ${state.currentUser.role}` : "Signed out";
    elements.logoutButton.classList.toggle("hidden", !isAuthenticated);
    elements.currentUser.classList.toggle("hidden", !isAuthenticated);

    if (isAuthenticated) {
        const department = state.currentUser.department ? `, ${state.currentUser.department.name}` : "";
        elements.currentUser.innerHTML = `
            <strong>${escapeHtml(state.currentUser.full_name)}</strong>
            <div>${escapeHtml(state.currentUser.username)}${department}</div>
        `;
    } else {
        elements.currentUser.innerHTML = "";
    }
}

async function loadPatients() {
    if (!ensureSignedIn()) return;

    try {
        const query = buildQuery(elements.patientsForm, { page: 1, limit: 10 });
        const response = await apiFetch(`/api/v1/patients?${query}`);
        const rows = response.data.items || [];
        state.patients = rows;
        rows.forEach((patient) => {
            state.patientLookup[patient.id] = patient;
        });

        if (rows.length === 0) {
            elements.patientsBody.innerHTML = `<tr><td colspan="6" class="empty-state">No patients matched these filters.</td></tr>`;
            return;
        }

        elements.patientsBody.innerHTML = rows.map((patient) => `
            <tr>
                <td>${escapeHtml(patient.mrn)}</td>
                <td>${escapeHtml(`${patient.first_name} ${patient.last_name}`)}</td>
                <td>${escapeHtml(patient.phone || "-")}</td>
                <td>${escapeHtml(patient.email || "-")}</td>
                <td><span class="pill ${patient.is_active ? "active" : "inactive"}">${patient.is_active ? "Active" : "Inactive"}</span></td>
                <td>
                    <div class="row-actions">
                        <button class="mini-button" type="button" data-edit-patient="${patient.id}">Edit</button>
                        <button class="mini-button" type="button" data-use-patient="${patient.id}" data-target="appointment">Use for appointment</button>
                        <button class="mini-button" type="button" data-use-patient="${patient.id}" data-target="prescription">Use for prescription</button>
                        <button class="mini-button" type="button" data-history-patient="${patient.id}">History</button>
                    </div>
                </td>
            </tr>
        `).join("");
    } catch (error) {
        setStatus(error.message, "error");
    }
}

async function loadAppointments() {
    if (!ensureSignedIn()) return;

    try {
        const query = buildQuery(elements.appointmentsForm, { page: 1, limit: 10 });
        const response = await apiFetch(`/api/v1/appointments?${query}`);
        state.appointments = response.data.items || [];
        await hydrateAppointmentLookups(state.appointments);
        renderAppointments();
    } catch (error) {
        setStatus(error.message, "error");
    }
}

async function loadPrescriptions() {
    if (!ensureSignedIn()) return;

    try {
        const query = buildQuery(elements.prescriptionsForm, { page: 1, limit: 10 });
        const response = await apiFetch(`/api/v1/prescriptions?${query}`);
        const rows = response.data.items || [];
        state.prescriptions = rows;
        await hydrateAppointmentLookups(rows.map((prescription) => ({
            patient_id: prescription.patient_id,
            doctor_id: prescription.doctor_id
        })));

        if (rows.length === 0) {
            elements.prescriptionsList.innerHTML = `<p class="empty-state">No prescriptions matched these filters.</p>`;
            renderAppointments();
            return;
        }

        elements.prescriptionsList.innerHTML = rows.map((prescription) => `
            <article class="prescription-card" data-prescription-id="${prescription.id}">
                <h4>Prescription #${prescription.id}</h4>
                <p>${escapeHtml(getPatientLabel(prescription.patient_id))} | ${escapeHtml(getAppointmentLabel(prescription.appointment_id))} | ${escapeHtml(getDoctorLabel(prescription.doctor_id))}</p>
                <p><strong>Diagnosis:</strong> ${escapeHtml(prescription.diagnosis || "-")}</p>
                <p><strong>Notes:</strong> ${escapeHtml(prescription.notes || "-")}</p>
                <ul>
                    ${(prescription.medicines || []).map((medicine) => `
                        <li>${escapeHtml(medicine.medicine_name)} ${escapeHtml(medicine.dosage || "")} - ${escapeHtml(medicine.frequency || "")}</li>
                    `).join("")}
                </ul>
                <div class="row-actions">
                    <button class="mini-button" type="button" data-edit-prescription="${prescription.id}">Edit</button>
                    <button class="mini-button" type="button" data-open-history="${prescription.patient_id}">View patient history</button>
                </div>
            </article>
        `).join("");
        renderAppointments();
    } catch (error) {
        setStatus(error.message, "error");
    }
}

async function savePrescription() {
    if (!ensureSignedIn()) return;

    const role = (state.currentUser?.role || "").toUpperCase();
    if (role !== "DOCTOR") {
        setStatus("Prescription create and edit actions are available for doctor logins.", "error");
        return;
    }

    const form = elements.prescriptionEditorForm;
    const prescriptionId = form.elements.id.value;
    const medicines = collectMedicines();
    if (medicines.length === 0) {
        setStatus("Add at least one medicine item to the prescription.", "error");
        return;
    }

    try {
        if (prescriptionId) {
            await apiFetch(`/api/v1/prescriptions/${prescriptionId}`, {
                method: "PUT",
                headers: { "Content-Type": "application/json" },
                body: JSON.stringify({
                    notes: nullIfBlank(form.elements.notes.value),
                    diagnosis: nullIfBlank(form.elements.diagnosis.value),
                    medicines
                })
            });
            setStatus(`Prescription #${prescriptionId} updated successfully.`, "success");
        } else {
            await apiFetch("/api/v1/prescriptions", {
                method: "POST",
                headers: { "Content-Type": "application/json" },
                body: JSON.stringify({
                    appointment_id: Number(form.elements.appointment_id.value),
                    patient_id: Number(form.elements.patient_id.value),
                    notes: nullIfBlank(form.elements.notes.value),
                    diagnosis: nullIfBlank(form.elements.diagnosis.value),
                    medicines
                })
            });
            setStatus("Prescription created successfully.", "success");
        }

        resetPrescriptionEditor();
        await loadPrescriptions();
    } catch (error) {
        setStatus(error.message, "error");
    }
}

async function loadPatientHistory(patientIdOverride) {
    if (!ensureSignedIn()) return;

    const patientId = patientIdOverride || Number(elements.historyForm.elements.patient_id.value);
    if (!patientId) {
        setStatus("Enter a patient ID to load their track record.", "error");
        return;
    }

    elements.historyForm.elements.patient_id.value = patientId;

    try {
        const response = await apiFetch(`/api/v1/patients/${patientId}/history`);
        const history = response.data;
        state.patientLookup[history.patient.id] = history.patient;
        await hydrateAppointmentLookups([
            ...history.appointments.map((appointment) => ({
                patient_id: appointment.patient_id,
                doctor_id: appointment.doctor_id
            })),
            ...history.prescriptions.map((prescription) => ({
                patient_id: prescription.patient_id,
                doctor_id: prescription.doctor_id
            }))
        ]);

        elements.historyPanel.innerHTML = `
            <div class="history-shell">
                <article class="history-card">
                    <p class="eyebrow">Patient</p>
                    <h4>${escapeHtml(`${history.patient.first_name} ${history.patient.last_name}`)}</h4>
                    <p>MRN: ${escapeHtml(history.patient.mrn)} | Gender: ${escapeHtml(history.patient.gender)} | Phone: ${escapeHtml(history.patient.phone || "-")}</p>
                </article>

                <section class="history-section">
                    <h5>Appointments</h5>
                    ${history.appointments.length
                        ? history.appointments.map((appointment) => `
                            <article class="history-item">
                                <strong>${escapeHtml(appointment.appointment_date)}</strong>
                                <div>${escapeHtml(`${appointment.start_time} - ${appointment.end_time}`)} | ${escapeHtml(getDoctorLabel(appointment.doctor_id))} | ${escapeHtml(appointment.status)}</div>
                                <div>${escapeHtml(appointment.reason || "-")}</div>
                            </article>
                        `).join("")
                        : `<p class="empty-state">No appointments found.</p>`}
                </section>

                <section class="history-section">
                    <h5>Prescriptions</h5>
                    ${history.prescriptions.length
                        ? history.prescriptions.map((prescription) => `
                            <article class="history-item">
                                <strong>Prescription #${prescription.id}</strong>
                                <div>${escapeHtml(getAppointmentLabel(prescription.appointment_id))} | ${escapeHtml(getDoctorLabel(prescription.doctor_id))} | ${escapeHtml(formatDateTime(prescription.issued_at))}</div>
                                <div>Diagnosis: ${escapeHtml(prescription.diagnosis || "-")}</div>
                                <div>Medicines: ${(prescription.medicines || []).map((medicine) => escapeHtml(medicine.medicine_name)).join(", ") || "-"}</div>
                            </article>
                        `).join("")
                        : `<p class="empty-state">No prescriptions found.</p>`}
                </section>
            </div>
        `;

        setStatus(`Loaded patient #${patientId} history.`, "success");
    } catch (error) {
        setStatus(error.message, "error");
    }
}

async function loadAuditLogs() {
    if (!ensureSignedIn()) return;

    if ((state.currentUser?.role || "").toUpperCase() !== "ADMIN") {
        elements.auditBody.innerHTML = `<tr><td colspan="5" class="empty-state">Audit log access is available for admins only.</td></tr>`;
        return;
    }

    try {
        const query = buildQuery(elements.auditForm, { page: 1, limit: 10 });
        const response = await apiFetch(`/api/v1/audit-logs?${query}`);
        const rows = response.data.items || [];

        if (rows.length === 0) {
            elements.auditBody.innerHTML = `<tr><td colspan="5" class="empty-state">No audit records matched these filters.</td></tr>`;
            return;
        }

        elements.auditBody.innerHTML = rows.map((entry) => `
            <tr>
                <td>${escapeHtml(formatDateTime(entry.created_at))}</td>
                <td>${escapeHtml(entry.action)}</td>
                <td>${escapeHtml(`${entry.entity_type} #${entry.entity_id}`)}</td>
                <td>${escapeHtml(entry.user_id?.toString() || "System")}</td>
                <td>${escapeHtml(entry.ip_address || "-")}</td>
            </tr>
        `).join("");
    } catch (error) {
        setStatus(error.message, "error");
    }
}

async function apiFetch(url, options = {}, requireAuth = true) {
    const headers = new Headers(options.headers || {});

    if (requireAuth && state.token) {
        headers.set("Authorization", `Bearer ${state.token}`);
    }

    const response = await fetch(url, { ...options, headers });
    const payload = await response.json().catch(() => null);

    if (!response.ok || payload?.success === false) {
        throw new Error(payload?.message || "Request failed.");
    }

    return payload;
}

async function savePatient() {
    if (!ensureSignedIn()) return;

    const form = elements.patientEditorForm;
    const patientId = form.elements.id.value;
    const payload = {
        mrn: form.elements.mrn.value.trim(),
        first_name: form.elements.first_name.value.trim(),
        last_name: form.elements.last_name.value.trim(),
        date_of_birth: form.elements.date_of_birth.value,
        gender: form.elements.gender.value,
        phone: nullIfBlank(form.elements.phone.value),
        email: nullIfBlank(form.elements.email.value),
        address: nullIfBlank(form.elements.address.value),
        blood_group: nullIfBlank(form.elements.blood_group.value)
    };

    try {
        if (patientId) {
            await apiFetch(`/api/v1/patients/${patientId}`, {
                method: "PUT",
                headers: { "Content-Type": "application/json" },
                body: JSON.stringify({
                    first_name: payload.first_name,
                    last_name: payload.last_name,
                    date_of_birth: payload.date_of_birth,
                    gender: payload.gender,
                    phone: payload.phone,
                    email: payload.email,
                    address: payload.address,
                    blood_group: payload.blood_group,
                    emergency_contact_name: nullIfBlank(form.elements.emergency_contact_name.value),
                    emergency_contact_phone: nullIfBlank(form.elements.emergency_contact_phone.value),
                    is_active: form.elements.is_active.value === "true"
                })
            });
            setStatus(`Patient #${patientId} updated successfully.`, "success");
        } else {
            await apiFetch("/api/v1/patients", {
                method: "POST",
                headers: { "Content-Type": "application/json" },
                body: JSON.stringify(payload)
            });
            setStatus("Patient created successfully.", "success");
        }

        resetPatientEditor();
        await loadPatients();
    } catch (error) {
        setStatus(error.message, "error");
    }
}

async function saveAppointment() {
    if (!ensureSignedIn()) return;

    const form = elements.appointmentEditorForm;
    const appointmentId = form.elements.id.value;
    const payload = {
        patient_id: Number(form.elements.patient_id.value),
        doctor_id: Number(form.elements.doctor_id.value),
        department_id: Number(form.elements.department_id.value),
        appointment_date: form.elements.appointment_date.value,
        start_time: normalizeTime(form.elements.start_time.value),
        end_time: normalizeTime(form.elements.end_time.value),
        reason: nullIfBlank(form.elements.reason.value)
    };

    try {
        if (appointmentId) {
            await apiFetch(`/api/v1/appointments/${appointmentId}`, {
                method: "PUT",
                headers: { "Content-Type": "application/json" },
                body: JSON.stringify({
                    doctor_id: payload.doctor_id,
                    department_id: payload.department_id,
                    appointment_date: payload.appointment_date,
                    start_time: payload.start_time,
                    end_time: payload.end_time,
                    reason: payload.reason,
                    remarks: nullIfBlank(form.elements.remarks.value)
                })
            });
            setStatus(`Appointment #${appointmentId} updated successfully.`, "success");
        } else {
            await apiFetch("/api/v1/appointments", {
                method: "POST",
                headers: { "Content-Type": "application/json" },
                body: JSON.stringify(payload)
            });
            setStatus("Appointment created successfully.", "success");
        }

        resetAppointmentEditor();
        await loadAppointments();
    } catch (error) {
        setStatus(error.message, "error");
    }
}

async function updateAppointmentStatus() {
    if (!ensureSignedIn()) return;

    const form = elements.appointmentEditorForm;
    const appointmentId = form.elements.id.value;
    if (!appointmentId) {
        setStatus("Select an appointment to update its status.", "error");
        return;
    }

    try {
        await apiFetch(`/api/v1/appointments/${appointmentId}/status`, {
            method: "PATCH",
            headers: { "Content-Type": "application/json" },
            body: JSON.stringify({ status: form.elements.status.value })
        });
        setStatus(`Appointment #${appointmentId} status updated.`, "success");
        await loadAppointments();
    } catch (error) {
        setStatus(error.message, "error");
    }
}

function editPatient(patient) {
    const form = elements.patientEditorForm;
    form.elements.id.value = patient.id;
    form.elements.mrn.value = patient.mrn;
    form.elements.first_name.value = patient.first_name;
    form.elements.last_name.value = patient.last_name;
    form.elements.date_of_birth.value = patient.date_of_birth;
    form.elements.gender.value = patient.gender;
    form.elements.phone.value = patient.phone || "";
    form.elements.email.value = patient.email || "";
    form.elements.is_active.value = patient.is_active ? "true" : "false";
    elements.patientFormTitle.textContent = `Edit patient #${patient.id}`;
}

function editAppointment(appointment) {
    const form = elements.appointmentEditorForm;
    form.elements.id.value = appointment.id;
    form.elements.patient_id.value = appointment.patient_id;
    form.elements.doctor_id.value = appointment.doctor_id;
    form.elements.department_id.value = appointment.department_id;
    form.elements.appointment_date.value = appointment.appointment_date;
    form.elements.start_time.value = trimTime(appointment.start_time);
    form.elements.end_time.value = trimTime(appointment.end_time);
    form.elements.reason.value = appointment.reason || "";
    form.elements.remarks.value = appointment.remarks || "";
    form.elements.status.value = appointment.status;
    elements.appointmentFormTitle.textContent = `Edit appointment #${appointment.id}`;
}

function editPrescription(prescription) {
    const form = elements.prescriptionEditorForm;
    form.elements.id.value = prescription.id;
    form.elements.appointment_id.value = prescription.appointment_id;
    form.elements.patient_id.value = prescription.patient_id;
    form.elements.diagnosis.value = prescription.diagnosis || "";
    form.elements.notes.value = prescription.notes || "";
    renderMedicines(prescription.medicines || []);
    elements.prescriptionFormTitle.textContent = `Edit prescription #${prescription.id}`;
}

function handlePatientTableClick(event) {
    const editButton = event.target.closest("[data-edit-patient]");
    if (editButton) {
        const patientId = Number(editButton.dataset.editPatient);
        const patient = state.patients.find((item) => item.id === patientId);
        if (patient) {
            editPatient(patient);
        }
        return;
    }

    const useButton = event.target.closest("[data-use-patient]");
    if (useButton) {
        const patientId = Number(useButton.dataset.usePatient);
        const patient = state.patients.find((item) => item.id === patientId);
        if (!patient) return;

        if (useButton.dataset.target === "appointment") {
            usePatientForAppointment(patient);
        } else {
            usePatientForPrescription(patient);
        }
        return;
    }

    const historyButton = event.target.closest("[data-history-patient]");
    if (historyButton) {
        const patientId = Number(historyButton.dataset.historyPatient);
        if (patientId) {
            loadPatientHistory(patientId);
        }
    }
}

function handleAppointmentTableClick(event) {
    const editButton = event.target.closest("[data-edit-appointment]");
    if (editButton) {
        const appointmentId = Number(editButton.dataset.editAppointment);
        const appointment = state.appointments.find((item) => item.id === appointmentId);
        if (appointment) {
            editAppointment(appointment);
        }
        return;
    }

    const useButton = event.target.closest("[data-use-appointment]");
    if (useButton) {
        const appointmentId = Number(useButton.dataset.useAppointment);
        const appointment = state.appointments.find((item) => item.id === appointmentId);
        if (appointment) {
            useAppointmentForPrescription(appointment);
        }
    }
}

function handlePrescriptionListClick(event) {
    const editButton = event.target.closest("[data-edit-prescription]");
    if (editButton) {
        const prescriptionId = Number(editButton.dataset.editPrescription);
        const prescription = state.prescriptions.find((item) => item.id === prescriptionId);
        if (prescription) {
            editPrescription(prescription);
        }
        return;
    }

    const historyButton = event.target.closest("[data-open-history]");
    if (historyButton) {
        const patientId = Number(historyButton.dataset.openHistory);
        if (patientId) {
            loadPatientHistory(patientId);
        }
    }
}

function resetPatientEditor() {
    elements.patientEditorForm.reset();
    elements.patientEditorForm.elements.id.value = "";
    elements.patientEditorForm.elements.gender.value = "MALE";
    elements.patientEditorForm.elements.is_active.value = "true";
    elements.patientFormTitle.textContent = "Create patient";
}

function resetAppointmentEditor() {
    elements.appointmentEditorForm.reset();
    elements.appointmentEditorForm.elements.id.value = "";
    elements.appointmentEditorForm.elements.status.value = "SCHEDULED";
    elements.appointmentFormTitle.textContent = "Create appointment";
    elements.appointmentSelection.textContent = "Tip: choose a patient from the search table to fill `patient_id` automatically.";
}

function resetPrescriptionEditor() {
    elements.prescriptionEditorForm.reset();
    elements.prescriptionEditorForm.elements.id.value = "";
    renderMedicines([
        { medicine_name: "", dosage: "", frequency: "", duration_days: "", instructions: "" }
    ]);
    elements.prescriptionFormTitle.textContent = "Create prescription for an appointment";
    elements.prescriptionSelection.textContent = "Tip: choose an appointment from the schedule to fill both appointment and patient automatically.";
}

function usePatientForAppointment(patient) {
    elements.appointmentEditorForm.elements.patient_id.value = patient.id;
    elements.appointmentSelection.textContent = `Selected patient #${patient.id}: ${patient.first_name} ${patient.last_name}`;
    setStatus(`Patient #${patient.id} is ready for the appointment form.`, "success");
}

function usePatientForPrescription(patient) {
    elements.prescriptionEditorForm.elements.patient_id.value = patient.id;
    elements.historyForm.elements.patient_id.value = patient.id;
    elements.prescriptionSelection.textContent = `Selected patient #${patient.id}: ${patient.first_name} ${patient.last_name}. Now choose an appointment from the schedule.`;
    setStatus(`Patient #${patient.id} is ready for the prescription form.`, "success");
}

function useAppointmentForPrescription(appointment) {
    elements.prescriptionEditorForm.elements.appointment_id.value = appointment.id;
    elements.prescriptionEditorForm.elements.patient_id.value = appointment.patient_id;
    elements.historyForm.elements.patient_id.value = appointment.patient_id;
    const hasPrescription = appointmentHasPrescription(appointment.id);
    elements.prescriptionSelection.textContent = hasPrescription
        ? `Appointment #${appointment.id} already has a prescription attached. Editing or adding another prescription will use patient #${appointment.patient_id}.`
        : `Selected appointment #${appointment.id} for patient #${appointment.patient_id}.`;
    setStatus(`Appointment #${appointment.id} is ready for the prescription form.`, "success");
}

function renderAppointments() {
    const rows = state.appointments || [];

    if (rows.length === 0) {
        elements.appointmentsBody.innerHTML = `<tr><td colspan="7" class="empty-state">No appointments matched these filters.</td></tr>`;
        return;
    }

    elements.appointmentsBody.innerHTML = rows.map((appointment) => {
        const hasPrescription = appointmentHasPrescription(appointment.id);
        const patientName = getPatientLabel(appointment.patient_id);
        const doctorName = getDoctorLabel(appointment.doctor_id);
        return `
            <tr>
                <td>${escapeHtml(appointment.appointment_date)}</td>
                <td>${escapeHtml(`${appointment.start_time} - ${appointment.end_time}`)}</td>
                <td>${escapeHtml(patientName)}</td>
                <td>${escapeHtml(doctorName)}</td>
                <td><span class="pill ${hasPrescription ? "linked" : "unlinked"}">${hasPrescription ? "Attached" : "Not linked"}</span></td>
                <td><span class="pill ${appointment.status.toLowerCase()}">${escapeHtml(appointment.status)}</span></td>
                <td>
                    <div class="row-actions">
                        <button class="mini-button" type="button" data-edit-appointment="${appointment.id}">Edit</button>
                        <button class="mini-button" type="button" data-use-appointment="${appointment.id}">Use for prescription</button>
                    </div>
                </td>
            </tr>
        `;
    }).join("");
}

function appointmentHasPrescription(appointmentId) {
    return state.prescriptions.some((prescription) => prescription.appointment_id === appointmentId);
}

async function hydrateAppointmentLookups(appointments) {
    const missingPatientIds = uniqueIds(appointments.map((appointment) => appointment.patient_id))
        .filter((id) => !state.patientLookup[id]);
    const missingDoctorIds = uniqueIds(appointments.map((appointment) => appointment.doctor_id))
        .filter((id) => !state.doctorLookup[id]);

    await Promise.all([
        ...missingPatientIds.map((id) => fetchPatientLookup(id)),
        ...missingDoctorIds.map((id) => fetchDoctorLookup(id))
    ]);
}

async function fetchPatientLookup(id) {
    try {
        const response = await apiFetch(`/api/v1/patients/${id}`);
        state.patientLookup[id] = response.data;
    } catch {
        state.patientLookup[id] = { id, first_name: "Unknown", last_name: "Patient" };
    }
}

async function fetchDoctorLookup(id) {
    try {
        const response = await apiFetch(`/api/v1/doctors/${id}`);
        state.doctorLookup[id] = response.data;
    } catch {
        state.doctorLookup[id] = { id, full_name: `Doctor #${id}` };
    }
}

function getPatientLabel(id) {
    const patient = state.patientLookup[id];
    if (!patient) {
        return `Patient #${id}`;
    }

    const fullName = `${patient.first_name || ""} ${patient.last_name || ""}`.trim();
    return fullName ? `${fullName} (#${id})` : `Patient #${id}`;
}

function getDoctorLabel(id) {
    const doctor = state.doctorLookup[id];
    if (!doctor) {
        return `Doctor #${id}`;
    }

    return doctor.full_name ? `${doctor.full_name} (#${id})` : `Doctor #${id}`;
}

function getAppointmentLabel(id) {
    const appointment = state.appointments.find((item) => item.id === id);
    if (!appointment) {
        return `Appointment #${id}`;
    }

    const patientLabel = getPatientLabel(appointment.patient_id);
    return `${patientLabel} on ${appointment.appointment_date}`;
}

function uniqueIds(values) {
    return [...new Set(values.filter(Boolean))];
}

function buildQuery(form, defaults = {}) {
    const params = new URLSearchParams();
    const formData = new FormData(form);

    for (const [key, value] of formData.entries()) {
        const normalized = String(value).trim();
        if (normalized) {
            params.set(key, normalized);
        }
    }

    Object.entries(defaults).forEach(([key, value]) => params.set(key, String(value)));
    return params.toString();
}

function nullIfBlank(value) {
    const normalized = String(value || "").trim();
    return normalized ? normalized : null;
}

function normalizeTime(value) {
    return value && value.length === 5 ? `${value}:00` : value;
}

function trimTime(value) {
    return value ? String(value).slice(0, 5) : "";
}

function addMedicineRow(medicine = {}) {
    const row = document.createElement("div");
    row.className = "medicine-row";
    row.innerHTML = `
        <input data-field="medicine_name" placeholder="Medicine name" value="${escapeAttribute(medicine.medicine_name || "")}">
        <input data-field="dosage" placeholder="Dosage" value="${escapeAttribute(medicine.dosage || "")}">
        <input data-field="frequency" placeholder="Frequency" value="${escapeAttribute(medicine.frequency || "")}">
        <input data-field="duration_days" type="number" min="1" placeholder="Days" value="${escapeAttribute(medicine.duration_days || "")}">
        <input data-field="instructions" placeholder="Instructions" value="${escapeAttribute(medicine.instructions || "")}">
        <button class="ghost-button" type="button" data-remove-medicine>Remove</button>
    `;

    row.querySelector("[data-remove-medicine]").addEventListener("click", () => {
        row.remove();
        if (!elements.medicinesEditor.children.length) {
            addMedicineRow();
        }
    });

    elements.medicinesEditor.appendChild(row);
}

function renderMedicines(medicines) {
    elements.medicinesEditor.innerHTML = "";
    const source = medicines.length ? medicines : [{}];
    source.forEach((medicine) => addMedicineRow(medicine));
}

function collectMedicines() {
    return Array.from(elements.medicinesEditor.querySelectorAll(".medicine-row"))
        .map((row) => ({
            medicine_name: row.querySelector('[data-field="medicine_name"]').value.trim(),
            dosage: row.querySelector('[data-field="dosage"]').value.trim(),
            frequency: row.querySelector('[data-field="frequency"]').value.trim(),
            duration_days: parseOptionalInteger(row.querySelector('[data-field="duration_days"]').value),
            instructions: nullIfBlank(row.querySelector('[data-field="instructions"]').value)
        }))
        .filter((medicine) => medicine.medicine_name && medicine.dosage && medicine.frequency);
}

function parseOptionalInteger(value) {
    const normalized = String(value || "").trim();
    return normalized ? Number(normalized) : null;
}

function escapeAttribute(value) {
    return escapeHtml(value).replaceAll('"', "&quot;");
}

function ensureSignedIn() {
    if (state.token && state.currentUser) {
        return true;
    }

    setStatus("Please sign in first.", "error");
    return false;
}

function setStatus(message, tone = "") {
    elements.statusPanel.textContent = message;
    elements.statusPanel.className = `status-panel ${tone}`.trim();
}

function formatDateTime(value) {
    if (!value) return "-";

    const parsed = new Date(value);
    if (Number.isNaN(parsed.getTime())) {
        return value;
    }

    return parsed.toLocaleString();
}

function escapeHtml(value) {
    return String(value)
        .replaceAll("&", "&amp;")
        .replaceAll("<", "&lt;")
        .replaceAll(">", "&gt;")
        .replaceAll('"', "&quot;")
        .replaceAll("'", "&#39;");
}

resetPrescriptionEditor();
