
#region Includes
using System;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Linq;
using System.ComponentModel;

using UnityEngine;
#endregion

/// <summary>
/// Class representing a controlling container for a 2D physical simulation
/// of a car with 5 front facing sensors, detecting the distance to obstacles.
/// </summary>
public class CarController : MonoBehaviour
{
    #region Members
    #region IDGenerator
    // Used for unique ID generation
    private static int idGenerator = 0;
    /// <summary>
    /// Returns the next unique id in the sequence.
    /// </summary>
    private static int NextID
    {
        get { return idGenerator++; }
    }
    #endregion

    // Maximum delay in seconds between the collection of two checkpoints until this car dies.
    private const float MAX_CHECKPOINT_DELAY = 7;
    StringBuilder sb = new StringBuilder();
    private bool DataCreationMode = false;

    /// <summary>
    /// The underlying AI agent of this car.
    /// </summary>
    public Agent Agent
    {
        get;
        set;
    }

    public float CurrentCompletionReward
    {
        get { return Agent.Genotype.Evaluation; }
        set { Agent.Genotype.Evaluation = value; }
    }

    /// <summary>
    /// Whether this car is controllable by user input (keyboard).
    /// </summary>
    public bool UseUserInput = true;

    /// <summary>
    /// The movement component of this car.
    /// </summary>
    public CarMovement Movement
    {
        get;
        private set;
    }

    /// <summary>
    /// The current inputs for controlling the CarMovement component.
    /// </summary>
    public double[] CurrentControlInputs
    {
        get { return Movement.CurrentInputs; }
    }

    /// <summary>
    /// The cached SpriteRenderer of this car.
    /// </summary>
    public SpriteRenderer SpriteRenderer
    {
        get;
        private set;
    }

    private Sensor[] sensors;
    private float timeSinceLastCheckpoint;
    #endregion

    #region Constructors
    void Awake()
    {
        //Cache components
        Movement = GetComponent<CarMovement>();
        SpriteRenderer = GetComponent<SpriteRenderer>();
        sensors = GetComponentsInChildren<Sensor>();
    }
    void Start()
    {
        Movement.HitWall += Die;

        //Set name to be unique
        this.name = "Car (" + NextID + ")";
    }
    #endregion

    #region Methods
    /// <summary>
    /// Restarts this car, making it movable again.
    /// </summary>
    public void Restart()
    {
        Movement.enabled = true;
        timeSinceLastCheckpoint = 0;

        foreach (Sensor s in sensors)
            s.Show();

        Agent.Reset();
        this.enabled = true;
    }

    // Unity method for normal update
    void Update()
    {
        timeSinceLastCheckpoint += Time.deltaTime;
    }

    // Unity method for physics update
    void FixedUpdate()
    {
        //Get control inputs from Agent
        if (!UseUserInput)
        {
            //Get readings from sensors
            double[] sensorOutput = new double[sensors.Length];
            for (int i = 0; i < sensors.Length; i++)
                sensorOutput[i] = sensors[i].Output;

            //OLD double[] controlInputs = Agent.FNN.ProcessInputs(sensorOutput);
            double controlInput = 0;
            if (DataCreationMode)
            {
                if (Input.GetKey("left"))
                {
                    controlInput = -1;
                }
                else if (Input.GetKey("right"))
                {
                    controlInput = 1;
                }
                sb.Append(sensorOutput[0] + "," + sensorOutput[1] + "," + sensorOutput[2] + "," + sensorOutput[3] + "," + sensorOutput[4] + "," + controlInput + "\r\n");
                Movement.SetInputs(controlInput);
                if (Input.GetKey("up")) File.AppendAllText(@"C:\Users\Família\Desktop\UnityProject_discrete\log.txt", sb.ToString());
                if (Input.GetKey("down")) sb = new StringBuilder();
            }
            else
            {
                //Calling Rstudio
                var rpath = @"C:\Program Files\R\R-3.6.0\bin\Rscript.exe";//CAMBIAR
                var scriptpath = @"C:\Users\Família\Desktop\UnityProject_discrete\NN_readr.R";//CAMBIAR String.Join(" ", sensorOutput.Select(x=> x.ToString()).ToArray())
                string inputsNN = string.Format("{0} {1} {2} {3} {4}", sensorOutput[0], sensorOutput[1], sensorOutput[2], sensorOutput[3], sensorOutput[4]);
                var controlInputPrueba = RunFromCmd(rpath, scriptpath,  inputsNN);
                //UnityEngine.Debug.Log(controlInputPrueba);
                string[] aux = controlInputPrueba.Split(' ');
                UnityEngine.Debug.Log(Int32.Parse(aux[1]));
                Movement.SetInputs(Int32.Parse(aux[1]));
            }
        }
        
        if (timeSinceLastCheckpoint > MAX_CHECKPOINT_DELAY)
        {
            Die();

        }
    }
    //Calls R to retrieve the result from the NN.
    public static string RunFromCmd(string rCodeFilePath, string rScriptExecutablePath, string args)
    {
        string file = rCodeFilePath;
        string result = string.Empty;

        try
        {

            var info = new ProcessStartInfo();
            info.FileName = rCodeFilePath;
            info.WorkingDirectory = Path.GetDirectoryName(rScriptExecutablePath);
            info.Arguments = rScriptExecutablePath + " " + args;

            info.RedirectStandardInput = false;
            info.RedirectStandardOutput = true;
            info.UseShellExecute = false;
            info.CreateNoWindow = true;

            using (var proc = new Process())
            {
                proc.StartInfo = info;
                proc.Start();
                result = proc.StandardOutput.ReadToEnd();

            }

            return result;
        }
        catch (Exception ex)
        {
            throw new Exception("R Script failed: " + result, ex);
        }
    }


    // Makes this car die (making it unmovable and stops the Agent from calculating the controls for the car).
    private void Die()
    {
        this.enabled = false;
        Movement.Stop();
        Movement.enabled = false;

        foreach (Sensor s in sensors)
            s.Hide();

        Agent.Kill();
        //New: save the log file
        //File.AppendAllText(@"C:\Users\Família\Desktop\UnityProject\log.txt", sb.ToString());
        //sb = new StringBuilder();
    }

    public void CheckpointCaptured()
    {
        timeSinceLastCheckpoint = 0;
    }
    #endregion
}
