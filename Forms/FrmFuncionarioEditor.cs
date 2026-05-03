using System;
using System.Drawing;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using GestionFuncionarios.DAO;
using GestionFuncionarios.Exceptions;
using GestionFuncionarios.Models;

namespace GestionFuncionarios.Forms
{
    /// <summary>
    /// Formulario para crear o editar un Funcionario.
    /// Si recibe null, opera en modo "Crear"; en caso contrario, "Editar".
    /// </summary>
    public class FrmFuncionarioEditor : Form
    {
        private readonly IFuncionarioDAO _funcionarioDAO = new FuncionarioDAO();
        private readonly LookupDAO       _lookupDAO      = new LookupDAO();
        private readonly Funcionario?    _funcionarioOriginal;
        private readonly bool            _esEdicion;

        // Controles
        private ComboBox       cboTipoDocumento = null!;
        private TextBox        txtNumeroDocumento = null!;
        private TextBox        txtNombres = null!;
        private TextBox        txtApellidos = null!;
        private TextBox        txtEmail = null!;
        private TextBox        txtTelefono = null!;
        private TextBox        txtDireccion = null!;
        private DateTimePicker dtpFechaNacimiento = null!;
        private DateTimePicker dtpFechaIngreso = null!;
        private ComboBox       cboCargo = null!;
        private ComboBox       cboDependencia = null!;
        private NumericUpDown  numSalario = null!;
        private CheckBox       chkActivo = null!;
        private Button         btnGuardar = null!;
        private Button         btnCancelar = null!;

        public FrmFuncionarioEditor(Funcionario? funcionario)
        {
            _funcionarioOriginal = funcionario;
            _esEdicion           = funcionario != null;

            ConstruirInterfaz();
            CargarLookups();

            if (_esEdicion && _funcionarioOriginal != null)
                CargarDatos(_funcionarioOriginal);
        }

        // -------------------------------------------------------------
        // Construcción de la interfaz
        // -------------------------------------------------------------
        private void ConstruirInterfaz()
        {
            Text            = _esEdicion ? "Editar Funcionario" : "Nuevo Funcionario";
            Width           = 560;
            Height          = 620;
            StartPosition   = FormStartPosition.CenterParent;
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox     = false;
            MinimizeBox     = false;

            var tabla = new TableLayoutPanel
            {
                Dock        = DockStyle.Top,
                ColumnCount = 2,
                RowCount    = 13,
                Padding     = new Padding(15),
                AutoSize    = true
            };
            tabla.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 150));
            tabla.ColumnStyles.Add(new ColumnStyle(SizeType.Percent,  100));

            // Inicialización de controles
            cboTipoDocumento   = new ComboBox       { DropDownStyle = ComboBoxStyle.DropDownList, Width = 340 };
            txtNumeroDocumento = new TextBox        { Width = 340, MaxLength = 20 };
            txtNombres         = new TextBox        { Width = 340, MaxLength = 100 };
            txtApellidos       = new TextBox        { Width = 340, MaxLength = 100 };
            txtEmail           = new TextBox        { Width = 340, MaxLength = 100 };
            txtTelefono        = new TextBox        { Width = 340, MaxLength = 20 };
            txtDireccion       = new TextBox        { Width = 340, MaxLength = 200 };
            dtpFechaNacimiento = new DateTimePicker { Width = 340, Format = DateTimePickerFormat.Short, Value = DateTime.Today.AddYears(-25) };
            dtpFechaIngreso    = new DateTimePicker { Width = 340, Format = DateTimePickerFormat.Short, Value = DateTime.Today };
            cboCargo           = new ComboBox       { DropDownStyle = ComboBoxStyle.DropDownList, Width = 340 };
            cboDependencia     = new ComboBox       { DropDownStyle = ComboBoxStyle.DropDownList, Width = 340 };
            numSalario         = new NumericUpDown  { Width = 340, Maximum = 999_999_999, Minimum = 0, ThousandsSeparator = true, DecimalPlaces = 0 };
            chkActivo          = new CheckBox       { Text = "Funcionario activo", Checked = true, AutoSize = true };

            int fila = 0;
            AgregarFila(tabla, fila++, "Tipo de documento *", cboTipoDocumento);
            AgregarFila(tabla, fila++, "Número documento *",  txtNumeroDocumento);
            AgregarFila(tabla, fila++, "Nombres *",           txtNombres);
            AgregarFila(tabla, fila++, "Apellidos *",         txtApellidos);
            AgregarFila(tabla, fila++, "Correo electrónico *", txtEmail);
            AgregarFila(tabla, fila++, "Teléfono",            txtTelefono);
            AgregarFila(tabla, fila++, "Dirección",           txtDireccion);
            AgregarFila(tabla, fila++, "Fecha nacimiento *",  dtpFechaNacimiento);
            AgregarFila(tabla, fila++, "Fecha ingreso *",     dtpFechaIngreso);
            AgregarFila(tabla, fila++, "Cargo *",             cboCargo);
            AgregarFila(tabla, fila++, "Dependencia *",       cboDependencia);
            AgregarFila(tabla, fila++, "Salario *",           numSalario);
            AgregarFila(tabla, fila++, "Estado",              chkActivo);

            // Panel de botones
            var panelBotones = new FlowLayoutPanel
            {
                Dock          = DockStyle.Bottom,
                Height        = 60,
                FlowDirection = FlowDirection.RightToLeft,
                Padding       = new Padding(15)
            };

            btnCancelar = new Button
            {
                Text      = "Cancelar",
                Width     = 100,
                Height    = 35,
                BackColor = Color.Gray,
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Margin    = new Padding(5)
            };
            btnCancelar.FlatAppearance.BorderSize = 0;
            btnCancelar.Click += (_, _) => { DialogResult = DialogResult.Cancel; Close(); };

            btnGuardar = new Button
            {
                Text      = "Guardar",
                Width     = 100,
                Height    = 35,
                BackColor = Color.SeaGreen,
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font      = new Font("Segoe UI", 9.5F, FontStyle.Bold),
                Margin    = new Padding(5)
            };
            btnGuardar.FlatAppearance.BorderSize = 0;
            btnGuardar.Click += BtnGuardar_Click;

            panelBotones.Controls.Add(btnGuardar);
            panelBotones.Controls.Add(btnCancelar);

            Controls.Add(tabla);
            Controls.Add(panelBotones);

            AcceptButton = btnGuardar;
            CancelButton = btnCancelar;
        }

        private static void AgregarFila(TableLayoutPanel t, int fila, string etiqueta, Control control)
        {
            t.RowStyles.Add(new RowStyle(SizeType.Absolute, 35));
            t.Controls.Add(new Label
            {
                Text      = etiqueta,
                AutoSize  = false,
                Width     = 145,
                TextAlign = ContentAlignment.MiddleLeft,
                Margin    = new Padding(0, 6, 0, 0)
            }, 0, fila);
            t.Controls.Add(control, 1, fila);
        }

        // -------------------------------------------------------------
        // Carga de datos
        // -------------------------------------------------------------
        private void CargarLookups()
        {
            try
            {
                cboTipoDocumento.DataSource    = _lookupDAO.ListarTiposDocumento();
                cboTipoDocumento.DisplayMember = "Descripcion";
                cboTipoDocumento.ValueMember   = "IdTipoDocumento";

                cboCargo.DataSource    = _lookupDAO.ListarCargos();
                cboCargo.DisplayMember = "Nombre";
                cboCargo.ValueMember   = "IdCargo";

                cboDependencia.DataSource    = _lookupDAO.ListarDependencias();
                cboDependencia.DisplayMember = "Nombre";
                cboDependencia.ValueMember   = "IdDependencia";
            }
            catch (DAOException ex)
            {
                MessageBox.Show(ex.Message, "Error al cargar datos",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                Close();
            }
        }

        private void CargarDatos(Funcionario f)
        {
            cboTipoDocumento.SelectedValue   = f.IdTipoDocumento;
            txtNumeroDocumento.Text          = f.NumeroDocumento;
            txtNombres.Text                  = f.Nombres;
            txtApellidos.Text                = f.Apellidos;
            txtEmail.Text                    = f.Email;
            txtTelefono.Text                 = f.Telefono ?? string.Empty;
            txtDireccion.Text                = f.Direccion ?? string.Empty;
            dtpFechaNacimiento.Value         = f.FechaNacimiento;
            dtpFechaIngreso.Value            = f.FechaIngreso;
            cboCargo.SelectedValue           = f.IdCargo;
            cboDependencia.SelectedValue     = f.IdDependencia;
            numSalario.Value                 = f.Salario;
            chkActivo.Checked                = f.Activo;
        }

        // -------------------------------------------------------------
        // Guardar
        // -------------------------------------------------------------
        private void BtnGuardar_Click(object? sender, EventArgs e)
        {
            if (!Validar(out string mensajeError))
            {
                MessageBox.Show(mensajeError, "Validación",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            var funcionario = ConstruirFuncionario();

            try
            {
                if (_esEdicion)
                {
                    funcionario.IdFuncionario = _funcionarioOriginal!.IdFuncionario;
                    bool ok = _funcionarioDAO.Actualizar(funcionario);
                    if (!ok)
                    {
                        MessageBox.Show("No se encontró el funcionario para actualizar.",
                            "Aviso", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        return;
                    }
                    MessageBox.Show("Funcionario actualizado correctamente.", "Éxito",
                        MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                else
                {
                    int nuevoId = _funcionarioDAO.Crear(funcionario);
                    MessageBox.Show($"Funcionario creado correctamente. ID: {nuevoId}",
                        "Éxito", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }

                DialogResult = DialogResult.OK;
                Close();
            }
            catch (DAOException ex)
            {
                MessageBox.Show(ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error inesperado: " + ex.Message, "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private Funcionario ConstruirFuncionario() => new()
        {
            IdTipoDocumento  = (int)cboTipoDocumento.SelectedValue!,
            NumeroDocumento  = txtNumeroDocumento.Text.Trim(),
            Nombres          = txtNombres.Text.Trim(),
            Apellidos        = txtApellidos.Text.Trim(),
            Email            = txtEmail.Text.Trim(),
            Telefono         = string.IsNullOrWhiteSpace(txtTelefono.Text)  ? null : txtTelefono.Text.Trim(),
            Direccion        = string.IsNullOrWhiteSpace(txtDireccion.Text) ? null : txtDireccion.Text.Trim(),
            FechaNacimiento  = dtpFechaNacimiento.Value.Date,
            FechaIngreso     = dtpFechaIngreso.Value.Date,
            IdCargo          = (int)cboCargo.SelectedValue!,
            IdDependencia    = (int)cboDependencia.SelectedValue!,
            Salario          = numSalario.Value,
            Activo           = chkActivo.Checked
        };

        // -------------------------------------------------------------
        // Validación
        // -------------------------------------------------------------
        private bool Validar(out string mensajeError)
        {
            mensajeError = string.Empty;

            if (cboTipoDocumento.SelectedValue == null)
            { mensajeError = "Seleccione un tipo de documento."; return false; }

            if (string.IsNullOrWhiteSpace(txtNumeroDocumento.Text))
            { mensajeError = "El número de documento es obligatorio."; return false; }

            if (string.IsNullOrWhiteSpace(txtNombres.Text))
            { mensajeError = "Los nombres son obligatorios."; return false; }

            if (string.IsNullOrWhiteSpace(txtApellidos.Text))
            { mensajeError = "Los apellidos son obligatorios."; return false; }

            if (string.IsNullOrWhiteSpace(txtEmail.Text))
            { mensajeError = "El correo electrónico es obligatorio."; return false; }

            if (!EsEmailValido(txtEmail.Text.Trim()))
            { mensajeError = "El correo electrónico no tiene un formato válido."; return false; }

            if (dtpFechaNacimiento.Value.Date >= DateTime.Today)
            { mensajeError = "La fecha de nacimiento debe ser anterior a hoy."; return false; }

            int edad = DateTime.Today.Year - dtpFechaNacimiento.Value.Year;
            if (dtpFechaNacimiento.Value.Date > DateTime.Today.AddYears(-edad)) edad--;
            if (edad < 18)
            { mensajeError = "El funcionario debe ser mayor de edad (18 años o más)."; return false; }

            if (dtpFechaIngreso.Value.Date < dtpFechaNacimiento.Value.Date)
            { mensajeError = "La fecha de ingreso no puede ser anterior a la fecha de nacimiento."; return false; }

            if (cboCargo.SelectedValue == null)
            { mensajeError = "Seleccione un cargo."; return false; }

            if (cboDependencia.SelectedValue == null)
            { mensajeError = "Seleccione una dependencia."; return false; }

            if (numSalario.Value <= 0)
            { mensajeError = "El salario debe ser mayor a cero."; return false; }

            return true;
        }

        private static bool EsEmailValido(string email)
        {
            const string patron = @"^[^@\s]+@[^@\s]+\.[^@\s]+$";
            return Regex.IsMatch(email, patron);
        }
    }
}
