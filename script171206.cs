using Rhino;
using Rhino.Geometry;
using Rhino.DocObjects;
using Rhino.Collections;

using GH_IO;
using GH_IO.Serialization;
using Grasshopper;
using Grasshopper.Kernel;
using Grasshopper.Kernel.Data;
using Grasshopper.Kernel.Types;

using System;
using System.IO;
using System.Xml;
using System.Xml.Linq;
using System.Linq;
using System.Data;
using System.Drawing;
using System.Reflection;
using System.Collections;
using System.Windows.Forms;
using System.Collections.Generic;
using System.Runtime.InteropServices;


using Microsoft.CSharp.RuntimeBinder;



/// <summary>
/// This class will be instantiated on demand by the Script component.
/// </summary>
public class Script_Instance : GH_ScriptInstance
{
#region Utility functions
  /// <summary>Print a String to the [Out] Parameter of the Script component.</summary>
  /// <param name="text">String to print.</param>
  private void Print(string text) { /* Implementation hidden. */ }
  /// <summary>Print a formatted String to the [Out] Parameter of the Script component.</summary>
  /// <param name="format">String format.</param>
  /// <param name="args">Formatting parameters.</param>
  private void Print(string format, params object[] args) { /* Implementation hidden. */ }
  /// <summary>Print useful information about an object instance to the [Out] Parameter of the Script component. </summary>
  /// <param name="obj">Object instance to parse.</param>
  private void Reflect(object obj) { /* Implementation hidden. */ }
  /// <summary>Print the signatures of all the overloads of a specific method to the [Out] Parameter of the Script component. </summary>
  /// <param name="obj">Object instance to parse.</param>
  private void Reflect(object obj, string method_name) { /* Implementation hidden. */ }
#endregion

#region Members
  /// <summary>Gets the current Rhino document.</summary>
  private readonly RhinoDoc RhinoDocument;
  /// <summary>Gets the Grasshopper document that owns this script.</summary>
  private readonly GH_Document GrasshopperDocument;
  /// <summary>Gets the Grasshopper script component that owns this script.</summary>
  private readonly IGH_Component Component;
  /// <summary>
  /// Gets the current iteration count. The first call to RunScript() is associated with Iteration==0.
  /// Any subsequent call within the same solution will increment the Iteration count.
  /// </summary>
  private readonly int Iteration;
#endregion

  /// <summary>
  /// This procedure contains the user code. Input parameters are provided as regular arguments,
  /// Output parameters as ref arguments. You don't have to assign output parameters,
  /// they will have a default value.
  /// </summary>
  private void RunScript(bool Enable, int geneNum, int maxGeneration, double mutation, double elite, List<string> sliders, string target, ref object outgeneration, ref object outvalue)
  {
    if (!Enable)
    {
      return;
    }

    if (_form != null)
    {
      return;
    }

    _data = new OptData(GrasshopperDocument, sliders, geneNum, maxGeneration, elite, mutation, target);
    _form = OptForm.CreateNewForm(_data);
    Grasshopper.GUI.GH_WindowsFormUtil.CenterFormOnEditor(_form, true);
    _form.Show(Grasshopper.Instances.DocumentEditor);
    Grasshopper.Instances.DocumentEditor.FormShepard.RegisterForm(_form);


  }

  // <Custom additional code> 
  private static Form _form;
  private static OptData _data;

  // public static List<double> valLst = new List<double>{};
  public static List<Gene> geneLst = new List<Gene>();

  // "FormClosingHander()"
  private static void FormClosingHandler(object sender, EventArgs e)
  {
    _form = null;
    _data = null;
  }

  // -----"Gene" Class-----
  public class Gene
  {

    public List<decimal> genePattern;
    public double eval;

    public Gene(List<decimal> _genePattern, double _eval)
    {
      genePattern = _genePattern;
      eval = _eval;
    }

    public void setGenePattern(List<decimal> _genePattern)
    {
      this.genePattern = _genePattern;
    }

    public void setEvaluation(double _eval)
    {
      this.eval = _eval;
    }

    public void SetGene(decimal _val, int _loc)
    {
      this.genePattern[_loc] = _val;
    }

  }

  // -----"OptData" Class----- : data-preparation
  public class OptData
  {
    // Parameters
    public readonly Grasshopper.Kernel.Special.GH_NumberSlider[] Sliders;
    public readonly Grasshopper.Kernel.Parameters.Param_Number Parameter;
    public readonly GH_Document Document;
    public readonly int geneNum;
    public readonly int maxGeneration;

    // added on 171127
    public readonly double elite;
    public readonly double mutation;

    // "OptData.Optdata()"
    public OptData(GH_Document doc, IEnumerable<string> sliderNames, int _geneNum, int _maxGeneration, double _elite, double _mutation, string _targetName)
    {
      Document = doc;
      geneNum = _geneNum;
      maxGeneration = _maxGeneration;

      elite = _elite;
      mutation = _mutation;

      // find a single objective value:
      foreach(IGH_DocumentObject obj in doc.Objects)
      {
        var param = obj as Grasshopper.Kernel.Parameters.Param_Number;
        if (param == null)
          continue;
        if (param.NickName == _targetName)
        {
          Parameter = param;
          break;
        }
      }
      if (Parameter == null)
        throw new InvalidOperationException("Could not find target parameter: " + "Objective");

      // find sliders to apply changes:
      var localSliders = new List<Grasshopper.Kernel.Special.GH_NumberSlider>();
      foreach (string sliderName in sliderNames)
      {
        foreach (IGH_DocumentObject obj in doc.Objects)
        {
          var slider = obj as Grasshopper.Kernel.Special.GH_NumberSlider;
          if (slider == null)
            continue;
          if (slider.NickName == sliderName)
          {
            localSliders.Add(slider);
            break;
          }
        }
      }
      // Rhino.RhinoApp.WriteLine(Sliders.Length.ToString());
      Sliders = localSliders.ToArray();

      if (Sliders.Length == 0)
        throw new InvalidOperationException("Could not find any sliders");

      // obtain slider length and generate the first generation's genes
      for (int i = 0; i < _geneNum;i++)
      {
        Random rnd = new Random();
        List<decimal> randLst = new List<decimal>();

        for (int j = 0; j < Sliders.Length;j++)
        {
          decimal tmpVal = (decimal) rnd.Next(0, 1001) / 1000m;
          randLst.Add(tmpVal);
          System.Threading.Thread.Sleep(10);
          // ã‚ð‹²‚Ü‚È‚¢‚Æ‚È‚º‚©“¯‚¶Gene‚ª¶¬‚³‚ê‚éB
        }

        Gene tmpGene = new Gene(randLst, 0.0);
        geneLst.Add(tmpGene);

      }
    }
  }

  public static void Test(int _input, OptData _data)
  {
    MessageBox.Show(_input.ToString());
  }

  // -----"OptForm" Class----- : inheriting Windows form class
  public class OptForm : Form
  {

    // "OptForm" Parameters

    // private readonly OptData _data;
    private bool _abort = false;
    private Random _random = new Random();
    private int cntGene = 0;
    private int cntGeneration = 0;

    // "OptForm.CreateNewForm()"
    public static OptForm CreateNewForm(OptData data)
    {
      var form = new OptForm(data);

      form.FormClosing += FormClosingHandler;
      form.Width = 300;
      form.Height = 300;
      form.FormBorderStyle = FormBorderStyle.FixedToolWindow;
      form.ShowInTaskbar = false;
      form.StartPosition = FormStartPosition.Manual;

      var button = new Button();
      form.Controls.Add(button);
      button.Click += form.ButtonClick;
      button.Dock = DockStyle.Top;
      button.Text = "Start";
      form.Controls.Add(button);

      return form;
    }

    // "OptForm.OptForm()"
    private OptForm(OptData data)
    {

      if (data == null)
        throw new ArgumentNullException("data");

      _data = data;
    }

    // "OptForm.ButtonClick()"
    private void ButtonClick(object sender, EventArgs e)
    {

      Button button = sender as Button;
      if (button == null)
        return;

      if (button.Text == "Stop")
      {
        _abort = true;
        button.Text = "Start";
        _data.Document.NewSolution(false);
        return;
      }

      if (button.Text == "Start")
      {
        _abort = false;
        button.Text = "Stop";
        RunOptStep();
        return;
      }
      if (button.Text == "Done")
      {
        this.Close();
      }
    }

    // "OptForm.RunOptStep()": async - await
    public async void RunOptStep()
    {
      if (_abort)
        return;
      if (Grasshopper.Instances.ActiveCanvas.Document != _data.Document )
        return;

      if (cntGene == 0 && cntGeneration == 0)
      {
        // next gene's slider vals set:
        SetSlider(_data, geneLst, cntGene);
        _data.Document.NewSolution(false);
      }

      // set evaluation value
      try
      {
        double _objVal = GetObjective(_data.Document, "Objective");
        geneLst[cntGene].setEvaluation(_objVal);
      }
      catch (Exception e)
      {
        MessageBox.Show("SetEvaluation: " + e.Message);
      }

      // end step of each generation
      if (cntGene == _data.geneNum - 1)
      {
        // end of last generation
        if (cntGeneration == _data.maxGeneration - 1)
        {
          Button button = Controls[0] as Button;
          button.Text = "Done";

          // print
          Rhino.RhinoApp.WriteLine("--- simulation end ---");
          geneLst = SortGeneLst(geneLst);
          for (int i = 0; i < _data.geneNum; i++)
          {
            Rhino.RhinoApp.WriteLine(String.Join(",", geneLst[i].genePattern) + "=" + geneLst[i].eval.ToString());
          }

          // last gene's slider vals set:
          try
          {
            SetSlider(_data, geneLst, 0);
            _data.Document.NewSolution(false);
          }
          catch (Exception e)
          {
            MessageBox.Show("SetSlider: " + e.Message);
          }

          geneLst.Clear();

          return;
        }

        // sort

        List<Gene> sortedGeneLst = new List<Gene>();

        try
        {
          sortedGeneLst = SortGeneLst(geneLst);
        }
        catch (Exception e)
        {
          MessageBox.Show("SortGene: " + e.Message);
        }

        Rhino.RhinoApp.WriteLine("--- gen. " + cntGeneration.ToString() + " end ---");

        // geneLst = SortGeneLst(geneLst);
        for (int i = 0; i < _data.geneNum; i++)
        {
          Rhino.RhinoApp.WriteLine(String.Join(",", sortedGeneLst[i].genePattern) + "=" + sortedGeneLst[i].eval.ToString());
        }

        // cross-pollination
        List<Gene> crossbred = new List<Gene>();
        int crossBreedNum = (int) (_data.elite * _data.geneNum);

        try
        {
          crossbred = Crossbreed(sortedGeneLst, crossBreedNum);
        }
        catch (Exception e)
        {
          MessageBox.Show("CrossBreed: " + e.Message);
        }

        // mutation
        int mutationNum = (int) _data.mutation * (int) _data.geneNum;
        if (mutationNum == 0)
        {
          mutationNum = 1;
        }
        List<Gene> mutated = new List<Gene>();
        try
        {
          mutated = Mutation(sortedGeneLst, mutationNum);
        }
        catch (Exception e)
        {
          MessageBox.Show("Mutation: " + e.Message);
        }

        // create next generation
        geneLst.Clear();
        List<Gene> tmpLst = new List<Gene>();
        tmpLst.AddRange(crossbred);
        tmpLst.AddRange(mutated);
        int numFromCurrentGene = _data.geneNum - crossBreedNum * 2 - mutationNum;
        tmpLst.AddRange(sortedGeneLst.GetRange(0, numFromCurrentGene));

        // Next generation's List
        geneLst = tmpLst;

        cntGeneration += 1;
        cntGene = 0;

      }
      else
      {
        cntGene += 1;
      }

      // next gene's slider vals set:
      try
      {
        SetSlider(_data, geneLst, cntGene);
        _data.Document.NewSolution(false);
      }
      catch (Exception e)
      {
        MessageBox.Show("SetSlider: " + e.Message);
      }
      await WaitAWhile();
      // Application.DoEvents();
      Invoke(new Action(RunOptStep));
    }

    async System.Threading.Tasks.Task WaitAWhile()
    {
      await System.Threading.Tasks.Task.Delay(10);
    }

    // "OptForm.SetSlider()"
    private void SetSlider(OptData _data, List<Gene> _geneLst, int _cntGene)
    {
      for (int i = 0; i < _data.Sliders.Length;i++)
      {
        decimal sliderVal0 = _data.Sliders[i].Slider.Value;
        decimal sliderVal1 = sliderVal0;

        sliderVal1 = DenormalizeSlider(_data.Sliders[i], geneLst[_cntGene].genePattern[i]);
        _data.Sliders[i].Slider.Value = sliderVal1;
      }
    }

    // "OptForm.DenormalizeSlider()"
    private decimal DenormalizeSlider(Grasshopper.Kernel.Special.GH_NumberSlider _slider, decimal _normalizedVal)
    {
      decimal _denormalizedValue = 0.0m;
      decimal _min = _slider.Slider.Minimum;
      decimal _max = _slider.Slider.Maximum;
      decimal _valRange = _max - _min;
      decimal _shiftVal = 0.0m;

      if (_min < 0.0m)
      {
        _shiftVal = -1.0m * _min;
      }

      _denormalizedValue = _normalizedVal * _valRange - _shiftVal;

      return _denormalizedValue;
    }

    // "OptForm.GetObjective()"
    private double GetObjective(GH_Document _doc, string _paramStr)
    {
      double _value = 0.0;
      List < Grasshopper.Kernel.Parameters.Param_Number > _tmpLst = new List<Grasshopper.Kernel.Parameters.Param_Number>{};
      foreach (IGH_DocumentObject _obj in _doc.Objects)
      {
        var _param = _obj as Grasshopper.Kernel.Parameters.Param_Number;
        if (_param == null)
          continue;
        else if(_param.NickName.StartsWith(_paramStr))
        {
          _tmpLst.Add(_param);
          break;
        }
      }
      _value = double.Parse(_tmpLst[0].VolatileData.get_Branch(0)[0].ToString());
      return _value;
    }

    // "OptForm.SortGeneLst()"
    private List<Gene> SortGeneLst(List<Gene> _geneLst)
    {
      List<Gene> _tmpLst = new List<Gene>();
      _tmpLst = _geneLst.OrderByDescending(a => a.eval).ToList();
      return _tmpLst;
    }

    // "OptForm.Crossbreed()"
    private List<Gene> Crossbreed(List<Gene> _sortedGeneLst, int _num)
    {
      int _geneLength = _sortedGeneLst[0].genePattern.Count;
      List<Gene> _crossbredLst = new List<Gene>();


      int _cnt = 1;
      int _eqCnt = 0;

      Gene G1 = _sortedGeneLst[_cnt - 1];
      Gene G2 = _sortedGeneLst[_cnt];

      Random _rnd1 = new Random();

      while (_cnt < _num + 1)
      {
        List<decimal> _tmpLst1 = new List<decimal>();
        List<decimal> _tmpLst2 = new List<decimal>();

        // if number of slider is one, return nothing:
        if (_geneLength == 1)
        {
          return _crossbredLst;
        }

        else // n>=2
        {

          int _p1 = _rnd1.Next(0, _geneLength); // when _gL=7, _p1=0,1,2,3,4,5,6
          int _p2 = _rnd1.Next(_p1, _geneLength); // _p2=0,1,2,3,4,5,6 ~ 6
          //
          _tmpLst1.AddRange(G1.genePattern.GetRange(0, _p1));
          _tmpLst1.AddRange(G2.genePattern.GetRange(_p1, _p2 - _p1 + 1));
          _tmpLst1.AddRange(G1.genePattern.GetRange(_p2 + 1, _geneLength - _p2 - 1));
          //
          _tmpLst2.AddRange(G2.genePattern.GetRange(0, _p1 + 1));
          _tmpLst2.AddRange(G1.genePattern.GetRange(_p1 + 1, _p2 - _p1));
          _tmpLst2.AddRange(G2.genePattern.GetRange(_p2 + 1, _geneLength - _p2 - 1));
          //
          bool isEqual = false;
          for (int _i = 1; _i < _sortedGeneLst.Count; _i++)
          {
            if (_tmpLst1.SequenceEqual(_sortedGeneLst[_i].genePattern))
            {
              isEqual = true;
              _eqCnt++;
              break;
            }
          }

          if (_eqCnt > 100)
          {
            // Rhino.RhinoApp.WriteLine("same genes are found more than 100 times!");
            _eqCnt = 0;
            isEqual = false;
          }

          if (isEqual == false)
          {
            _crossbredLst.Add(new Gene(_tmpLst1, 0.0));
            _crossbredLst.Add(new Gene(_tmpLst2, 0.0));

            _cnt++;
            G1 = _sortedGeneLst[_cnt - 1];
            G2 = _sortedGeneLst[_cnt];
          }
        }

      }
      return _crossbredLst;
    }

    // "Opt.Form.Mutation()"
    private List<Gene> Mutation(List<Gene> _sortedGeneLst, int _num)
    {
      Random rnd = new Random();
      List<Gene> _mutationLst = new List<Gene>();
      _mutationLst = _sortedGeneLst.OrderBy(x => rnd.Next()).ToList().GetRange(0, _num);


      for (int _i = 0; _i < _mutationLst.Count; _i++)
      {
        List<decimal> _replaceLst = new List<decimal>();
        for (int _j = 0; _j < _mutationLst[0].genePattern.Count;_j++)
        {
          decimal tmpVal = (decimal) rnd.Next(0, 1001) / 1000m;
          _replaceLst.Add(tmpVal);
        }
        // MessageBox.Show(String.Join(",", _replaceLst));
        _mutationLst[_i].setGenePattern(_replaceLst);

        try{
          _mutationLst[_i].setGenePattern(_replaceLst);
        }

        catch(Exception e)
        {
          MessageBox.Show("Mutation setGenePattern: " + e.Message);
        }

      }

      // Mutation to alter the same gene
      string S = "";
      bool mflag = false;
      for (int _i = 0; _i < _sortedGeneLst.Count; _i++)
      {
        List<decimal> _tmpLst = _sortedGeneLst[_i].genePattern;
        for (int _j = _i + 1; _j < _sortedGeneLst.Count - 1; _j++)
        {
          if (_tmpLst.SequenceEqual(_sortedGeneLst[_j].genePattern) == true)
          {

            decimal tmpVal = (decimal) rnd.Next(0, 1001) / 1000m;
            mflag = true;
            Random tmp = new Random();
            try{
              _sortedGeneLst[_j].SetGene(tmpVal, tmp.Next(0, _tmpLst.Count));
            }

            catch(Exception e)
            {
              MessageBox.Show("Mutation SetGene: " + e.Message);
            }

            S += String.Join(",", _tmpLst) + " -> " + String.Join(",", _sortedGeneLst[_j].genePattern) + "\n";

          }
        }


      }
      if (mflag == true)
      {
        Rhino.RhinoApp.WriteLine(S);
      }

      return _mutationLst;
    }

  }

/*
_data : Type = OptData, GH_Document doc: _data.Document
*/

  /// <summary>
  /// This method will be called once every solution, before any calls to RunScript.
  /// </summary>
  public override void BeforeRunScript()
  {
  }
  /// <summary>
  /// This method will be called once every solution, after any calls to RunScript.
  /// </summary>
  public override void AfterRunScript()
  {
  }

  // </Custom additional code> 
}