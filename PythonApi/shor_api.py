from flask import Flask, request, jsonify
import qsharp
import time

qsharp.init(project_root=".")

app = Flask(__name__)

@app.route('/shor-factor', methods=['POST'])
def factor():
    data = request.json
    number = data['number']
    
    # Start the timer
    start_time = time.time()

    # Call the Q# operation
    result = qsharp.eval("Shor.FactorSemiprimeInteger.simulate(number='number')")

    elapsed = time.time() - start_time
    return jsonify({
        'result': result,
        'time': elapsed
    })

if __name__ == '__main__':
    app.run(host='0.0.0.0', port=5000)
