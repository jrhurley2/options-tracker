import { useState } from 'react';
import { importApi } from '../api/client';
import type { CsvImportResult } from '../types';

export default function Import() {
  const [file, setFile] = useState<File | null>(null);
  const [broker, setBroker] = useState<string>('Fidelity');
  const [account, setAccount] = useState<string>('');
  const [importing, setImporting] = useState(false);
  const [result, setResult] = useState<CsvImportResult | null>(null);

  const handleFileChange = (e: React.ChangeEvent<HTMLInputElement>) => {
    if (e.target.files && e.target.files[0]) {
      setFile(e.target.files[0]);
      setResult(null);
    }
  };

  const handleImport = async () => {
    if (!file || !account) {
      alert('Please select a file and enter an account name');
      return;
    }

    try {
      setImporting(true);
      const response = await importApi.uploadCsv(file, broker, account);
      setResult(response.data);
      
      if (response.data.success) {
        setFile(null);
        setAccount('');
        // Reset file input
        const fileInput = document.getElementById('file-upload') as HTMLInputElement;
        if (fileInput) fileInput.value = '';
      }
    } catch (err: any) {
      setResult({
        success: false,
        transactionsImported: 0,
        errors: [err.response?.data?.message || 'Import failed'],
        warnings: [],
        message: 'Import failed'
      });
    } finally {
      setImporting(false);
    }
  };

  return (
    <div className="space-y-6">
      <h1 className="text-3xl font-bold text-gray-900">Import Transactions</h1>

      <div className="bg-white rounded-lg shadow p-6 max-w-2xl">
        <div className="space-y-6">
          {/* Broker Selection */}
          <div>
            <label className="block text-sm font-medium text-gray-700 mb-2">
              Broker
            </label>
            <select
              value={broker}
              onChange={(e) => setBroker(e.target.value)}
              className="w-full px-4 py-2 border border-gray-300 rounded-lg focus:outline-none focus:ring-2 focus:ring-blue-500"
            >
              <option value="Fidelity">Fidelity</option>
              <option value="Schwab">Schwab</option>
            </select>
          </div>

          {/* Account Name */}
          <div>
            <label className="block text-sm font-medium text-gray-700 mb-2">
              Account Name
            </label>
            <input
              type="text"
              value={account}
              onChange={(e) => setAccount(e.target.value)}
              placeholder="e.g., My Brokerage Account"
              className="w-full px-4 py-2 border border-gray-300 rounded-lg focus:outline-none focus:ring-2 focus:ring-blue-500"
            />
          </div>

          {/* File Upload */}
          <div>
            <label className="block text-sm font-medium text-gray-700 mb-2">
              CSV File
            </label>
            <input
              id="file-upload"
              type="file"
              accept=".csv"
              onChange={handleFileChange}
              className="w-full px-4 py-2 border border-gray-300 rounded-lg focus:outline-none focus:ring-2 focus:ring-blue-500"
            />
            {file && (
              <p className="mt-2 text-sm text-gray-600">
                Selected: {file.name} ({(file.size / 1024).toFixed(2)} KB)
              </p>
            )}
          </div>

          {/* Import Button */}
          <button
            onClick={handleImport}
            disabled={!file || !account || importing}
            className={`w-full px-6 py-3 rounded-lg font-medium transition ${
              !file || !account || importing
                ? 'bg-gray-300 text-gray-500 cursor-not-allowed'
                : 'bg-blue-600 text-white hover:bg-blue-700'
            }`}
          >
            {importing ? 'Importing...' : 'Import Transactions'}
          </button>
        </div>
      </div>

      {/* Import Result */}
      {result && (
        <div className={`rounded-lg shadow p-6 ${
          result.success ? 'bg-green-50 border border-green-200' : 'bg-red-50 border border-red-200'
        }`}>
          <h2 className={`text-lg font-semibold mb-3 ${
            result.success ? 'text-green-900' : 'text-red-900'
          }`}>
            {result.success ? 'Import Successful' : 'Import Failed'}
          </h2>
          
          <p className={`mb-4 ${result.success ? 'text-green-800' : 'text-red-800'}`}>
            {result.message}
          </p>

          {result.success && (
            <p className="text-green-700">
              Imported {result.transactionsImported} transactions
            </p>
          )}

          {result.errors.length > 0 && (
            <div className="mt-4">
              <h3 className="font-medium text-red-900 mb-2">Errors:</h3>
              <ul className="list-disc list-inside space-y-1 text-red-800">
                {result.errors.map((error, index) => (
                  <li key={index} className="text-sm">{error}</li>
                ))}
              </ul>
            </div>
          )}

          {result.warnings.length > 0 && (
            <div className="mt-4">
              <h3 className="font-medium text-yellow-900 mb-2">Warnings:</h3>
              <ul className="list-disc list-inside space-y-1 text-yellow-800">
                {result.warnings.map((warning, index) => (
                  <li key={index} className="text-sm">{warning}</li>
                ))}
              </ul>
            </div>
          )}
        </div>
      )}

      {/* Instructions */}
      <div className="bg-blue-50 border border-blue-200 rounded-lg p-6">
        <h2 className="text-lg font-semibold text-blue-900 mb-3">Import Instructions</h2>
        <div className="space-y-3 text-sm text-blue-800">
          <p><strong>Fidelity:</strong> Export transactions from Fidelity as CSV. The file should include columns for Date, Action, Symbol, Quantity, Price, and Amount.</p>
          <p><strong>Schwab:</strong> Export transactions from Schwab as CSV. The file should include columns for Date, Action, Symbol, Description, Quantity, Price, and Amount.</p>
          <p className="pt-2"><strong>Note:</strong> Duplicate transactions will be automatically skipped.</p>
        </div>
      </div>
    </div>
  );
}
