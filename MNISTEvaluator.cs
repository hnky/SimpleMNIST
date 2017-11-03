using CNTK;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Numerics;

namespace SimpleMNIST
{
    class MNISTEvaluator
    {
        private static readonly DeviceDescriptor CpuDevice = DeviceDescriptor.CPUDevice;
        private Function _mnistFunction;
        private Variable _mnistInput;

        public MNISTEvaluator()
        {

            // Load the model.
            // This example requires the MNIST.model.
            string modelFilePath = "MNISTConvolution.model";
            if (!File.Exists(modelFilePath))
            {
                throw new FileNotFoundException(modelFilePath, string.Format("Error: The model '{0}' does not exist. Please follow instructions in README.md in <CNTK>/Examples/Image/Classification/ResNet to create the model.", modelFilePath));
            }

            _mnistFunction = Function.Load(modelFilePath, CpuDevice);

            // Get input variable. The model has only one single input.
            _mnistInput = _mnistFunction.Arguments.Single();
        }

        public int ExpectedImageInputSize => _mnistInput.Shape.TotalSize;

        /// <summary>
        /// Returns the digit represented by the imageData.
        /// </summary>
        public List<MNISTResult> Evaluate(Tensor<float> imageData)
        {
            try
            {
                // Get output variable
                Variable outputVar = _mnistFunction.Output;

                var inputDataMap = new Dictionary<Variable, Value>();
                var outputDataMap = new Dictionary<Variable, Value>();

                // Create input data map
                var inputVal = Value.CreateBatch(_mnistInput.Shape, imageData, CpuDevice);
                inputDataMap.Add(_mnistInput, inputVal);

                // Create output data map
                outputDataMap.Add(outputVar, null);

                // Start evaluation on the device
                _mnistFunction.Evaluate(inputDataMap, outputDataMap, CpuDevice);

                // Get evaluate result as dense output
                var outputVal = outputDataMap[outputVar];

                // The model has only one single output - a list of 10 floats
                // representing the likelihood of that index being the digit
                var outputData = outputVal.GetDenseData<float>(outputVar).Single();

                List<MNISTResult> results = new List<MNISTResult>(outputData.Count);
                for (int i = 0; i < outputData.Count; i++)
                {
                    results.Add(new MNISTResult() { Digit = i, Confidence = outputData[i] });
                }

                // sort so the highest confidence is first
                results.Sort((left, right) => right.Confidence.CompareTo(left.Confidence));

                return results;
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.ToString());
                return new List<MNISTResult>();
            }
        }
    }

    public class MNISTResult
    {
        public int Digit { get; set; }
        public float Confidence { get; set; }
    }
}
