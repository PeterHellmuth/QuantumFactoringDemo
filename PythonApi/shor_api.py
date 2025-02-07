from flask import Flask, request, jsonify
import qsharp

qsharp.init(project_root=".")
from qsharp.code.Shor import FactorSemiprimeInteger

app = Flask(__name__)

@app.route('/shor-factor', methods=['POST'])
def factor():
    data = request.json
    app.logger.debug(f"Received data: {data}")
    number = data['number']
    
    try:
        (p, q) = FactorSemiprimeInteger(int(number))
        app.logger.debug(f"Response data: {(p, q)}")
        return jsonify({
            "factors": [p, q],
            "product": p * q
        })
    except Exception as e:
        app.logger.error(f"Error processing request: {e}")
        return jsonify({"error": str(e)}), 500

if __name__ == '__main__':
    app.run(host='0.0.0.0', port=5000, threaded=False)