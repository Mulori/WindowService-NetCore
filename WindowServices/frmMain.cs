using System.Diagnostics;
using System.ServiceProcess;

namespace WindowServices
{
    public partial class frmMain : Form
    {
        private static string nameService { get; set; }
        private static string path { get; set; }
        private static string task { get; set; }
        private static string time { get; set; }

        List<string> listTarefas = new List<string>();
        List<string> listCaminhos = new List<string>();
        List<string> listTempos = new List<string>();

        public frmMain()
        {
            InitializeComponent();
            TrayMenuContext();

            if (!Directory.Exists(@"C:\ServiceR\Config"))
            {
                Directory.CreateDirectory(@"C:\ServiceR\Config");
            }

            if (!File.Exists(@"C:\ServiceR\Config\nameService.ini"))
            {
                File.Create(@"C:\ServiceR\Config\nameService.ini");
            }

            if (!File.Exists(@"C:\ServiceR\Config\pathTask.ini"))
            {
                File.Create(@"C:\ServiceR\Config\pathTask.ini");
            }

            if (!File.Exists(@"C:\ServiceR\Config\nameTask.ini"))
            {
                File.Create(@"C:\ServiceR\Config\nameTask.ini");
            }

            if (!File.Exists(@"C:\ServiceR\Config\timeTask.ini"))
            {
                File.Create(@"C:\ServiceR\Config\timeTask.ini");
            }

            try
            {
                nameService = File.ReadAllText(@"C:\ServiceR\Config\nameService.ini");
                path = File.ReadAllText(@"C:\ServiceR\Config\pathTask.ini");
                task = File.ReadAllText(@"C:\ServiceR\Config\nameTask.ini");
                time = File.ReadAllText(@"C:\ServiceR\Config\timeTask.ini");
            }
            catch { }
            

            if (string.IsNullOrEmpty(nameService) || string.IsNullOrEmpty(path) || string.IsNullOrEmpty(task))
            {
                list.Items.Add("Não foi configurado o arquivo de servico, e o processo será finalizado");
                list.Items.Add("");
                list.Items.Add(@"Configure o nome do serviço em: C:\ServiceR\Config");

                timer.Stop();

                btnStart.Enabled = false;
                btnStop.Enabled = false;

                return;
            }
            
            var paths = path.Split('|');
            var tasks = task.Split('|');
            var times = time.Split('|');

            foreach (var _task in tasks)
            {
                listTarefas.Add(_task);
            }

            foreach (var _path in paths)
            {
                listCaminhos.Add(_path);
            }

            foreach (var _time in times)
            {
                listTempos.Add(_time);
            }

            list.Items.Clear();
            list.Items.Add("Aguarde! Processando serviço solicitado...");

            btnStart.Enabled = false;
            btnStop.Enabled = false;

            OcultarForm();
        }

        private void btnStart_Click(object sender, EventArgs e)
        {
            btnStart.Enabled = false;
            btnStop.Enabled = true;

            timer.Start();
        }

        private void btnStop_Click(object sender, EventArgs e)
        {
            btnStart.Enabled = true;
            btnStop.Enabled = false;

            timer.Stop();
        }

        private void timer_Tick(object sender, EventArgs e)
        {
            ServiceController sc = new ServiceController(nameService);

            if ((sc.Status.Equals(ServiceControllerStatus.Stopped)) || (sc.Status.Equals(ServiceControllerStatus.StopPending)))
            {
                list.Items.Clear();
                list.Items.Add("Processando Serviço Configurado...");
                list.Items.Add($"O serviço ({nameService}) está parado... Ultima verificação: {DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss")}");

                foreach(var i in listTarefas)
                {
                    if(i != "")
                        Kill(i);
                }
            }
            else
            {
                list.Items.Clear();
                list.Items.Add("Processando Serviço Configurado...");
                list.Items.Add($"O serviço ({nameService}) está em execução... Ultima verificação: {DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss")}");

                int index = 0;
                foreach (var i in listCaminhos)
                {
                    var item = i.Replace("\r\n", string.Empty);

                    if (item != "")
                    {
                        var timeStart = listTempos[index].Replace("\r\n", string.Empty);

                        if (timeStart != "")
                        {
                            var periodo = timeStart.Split('-');
                            var inicio = periodo[0];
                            var fim = periodo[1];

                            DateTime dataAbertura = DateTime.Parse(DateTime.Now.ToString("yyyy-MM-dd") + " " + inicio);
                            DateTime dataFinalizacao = DateTime.Parse(DateTime.Now.ToString("yyyy-MM-dd") + " " + fim);

                            if (DateTime.Now >= dataAbertura && DateTime.Now <= dataFinalizacao)
                            {
                                var taskName = listTarefas[index].Replace("\r\n", string.Empty);
                                if (Process.GetProcessesByName(taskName).Length < 1)
                                    Start(i);
                            }
                        }
                        else
                        {
                            var taskName = listTarefas[index].Replace("\r\n", string.Empty);
                            if (Process.GetProcessesByName(taskName).Length < 1)
                                Start(i);
                        }
                        
                    }

                    index++;
                }
            }

            sc.Refresh();
        }

        public void Kill(string pross)
        {
            try
            {
                Process[] ProcList;
                ProcList = Process.GetProcessesByName(pross);
                ProcList[0].Kill();
            }
            catch
            {  }            
        }

        public void Start(string path)
        {
            Process.Start(path);
        }

        void AbrirForm()
        {
            this.Show();
            this.ShowInTaskbar = true;
            this.WindowState = FormWindowState.Normal;
            Icon.Visible = false;
        }

        void OcultarForm()
        {
            this.Hide();
            this.WindowState = FormWindowState.Minimized;
            this.ShowInTaskbar = false;
            Icon.Visible = true;
        }

        public void TrayMenuContext()
        {
            this.Icon.ContextMenuStrip = new System.Windows.Forms.ContextMenuStrip();
            this.Icon.ContextMenuStrip.Items.Add("Abrir", null, this.MenuOpen_Click);
            //this.Icon.ContextMenuStrip.Items.Add("Sair", null, this.MenuExit_Click);
        }

        void MenuOpen_Click(object sender, EventArgs e)
        {
            AbrirForm();
        }

        void MenuExit_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }

        public void btnMinimize_Click(object sender, EventArgs e)
        {
            OcultarForm();
        }

        public void notifyIcon_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            AbrirForm();
        }

        private void frmMain_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Control && e.KeyCode == Keys.F4)
            {
                if (MessageBox.Show("Deseja finalizar a aplicação Window Service?", "Atenção", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
                Environment.Exit(1);
            }
        }

        private void list_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Control && e.KeyCode == Keys.F4)
            {
                if (MessageBox.Show("Deseja finalizar a aplicação Window Service?", "Atenção", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
                    Environment.Exit(1);
            }
        }

        private void btnMinimize_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Control && e.KeyCode == Keys.F4)
            {
                if (MessageBox.Show("Deseja finalizar a aplicação Window Service?", "Atenção", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
                    Environment.Exit(1);
            }
        }
    }
}