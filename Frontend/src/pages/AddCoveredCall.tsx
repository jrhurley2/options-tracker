import { useState, useEffect } from 'react';
import { useNavigate, useSearchParams } from 'react-router-dom';
import { positionsApi, optionsApi } from '../api/client';
import type { Position } from '../types';

export default function AddCoveredCall() {
  const navigate = useNavigate();
  const [searchParams] = useSearchParams();
  const positionIdParam = searchParams.get('positionId');

  const [positions, setPositions] = useState<Position[]>([]);
  const [selectedPosition, setSelectedPosition] = useState<Position | null>(null);
  const [formData, setFormData] = useState({
    positionId: positionIdParam || '',
    strikePrice: '',
    expirationDate: '',
    contracts: '',
    premiumPerContract: '',
    notes: ''
  });
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);

  useEffect(() => {
    loadPositions();
  }, []);

  useEffect(() => {
    if (formData.positionId) {
      const position = positions.find(p => p.id === parseInt(formData.positionId));
      setSelectedPosition(position || null);
    }
  }, [formData.positionId, positions]);

  const loadPositions = async () => {
    try {
      const response = await positionsApi.getAll();
      setPositions(response.data.filter(p => p.quantity > 0));
      
      if (positionIdParam) {
        const position = response.data.find(p => p.id === parseInt(positionIdParam));
        setSelectedPosition(position || null);
      }
    } catch (err) {
      console.error(err);
    }
  };

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    
    if (!formData.positionId || !formData.strikePrice || !formData.expirationDate || 
        !formData.contracts || !formData.premiumPerContract) {
      setError('All required fields must be filled');
      return;
    }

    try {
      setLoading(true);
      setError(null);
      
      await optionsApi.createCoveredCall({
        positionId: parseInt(formData.positionId),
        strikePrice: parseFloat(formData.strikePrice),
        expirationDate: formData.expirationDate,
        contracts: parseInt(formData.contracts),
        premiumPerContract: parseFloat(formData.premiumPerContract),
        notes: formData.notes || undefined
      });

      navigate('/options');
    } catch (err: any) {
      setError(err.response?.data || 'Failed to create covered call');
    } finally {
      setLoading(false);
    }
  };

  const handleChange = (e: React.ChangeEvent<HTMLInputElement | HTMLSelectElement | HTMLTextAreaElement>) => {
    setFormData({
      ...formData,
      [e.target.name]: e.target.value
    });
  };

  const maxContracts = selectedPosition ? Math.floor(selectedPosition.quantity / 100) : 0;
  const totalPremium = formData.contracts && formData.premiumPerContract 
    ? parseInt(formData.contracts) * parseFloat(formData.premiumPerContract) * 100 
    : 0;

  return (
    <div className="space-y-6">
      <div>
        <h1 className="text-3xl font-bold text-gray-900">Sell Covered Call</h1>
        <p className="mt-2 text-gray-600">Sell call options against your stock position</p>
      </div>

      <div className="bg-white rounded-lg shadow p-6 max-w-2xl">
        <form onSubmit={handleSubmit} className="space-y-6">
          {error && (
            <div className="bg-red-50 border border-red-200 rounded-lg p-4">
              <p className="text-red-800 text-sm">{error}</p>
            </div>
          )}

          <div>
            <label htmlFor="positionId" className="block text-sm font-medium text-gray-700 mb-2">
              Stock Position *
            </label>
            <select
              id="positionId"
              name="positionId"
              value={formData.positionId}
              onChange={handleChange}
              className="w-full px-4 py-2 border border-gray-300 rounded-lg focus:outline-none focus:ring-2 focus:ring-blue-500"
              required
            >
              <option value="">Select a position...</option>
              {positions.map((position) => (
                <option key={position.id} value={position.id}>
                  {position.symbol} - {position.quantity} shares @ ${position.averageCost.toFixed(2)}
                </option>
              ))}
            </select>
            {selectedPosition && (
              <p className="mt-2 text-sm text-gray-600">
                Max contracts: {maxContracts} ({selectedPosition.quantity} shares available)
              </p>
            )}
          </div>

          <div className="grid grid-cols-2 gap-4">
            <div>
              <label htmlFor="strikePrice" className="block text-sm font-medium text-gray-700 mb-2">
                Strike Price *
              </label>
              <input
                type="number"
                id="strikePrice"
                name="strikePrice"
                value={formData.strikePrice}
                onChange={handleChange}
                placeholder="e.g., 155.00"
                min="0.01"
                step="0.01"
                className="w-full px-4 py-2 border border-gray-300 rounded-lg focus:outline-none focus:ring-2 focus:ring-blue-500"
                required
              />
            </div>

            <div>
              <label htmlFor="expirationDate" className="block text-sm font-medium text-gray-700 mb-2">
                Expiration Date *
              </label>
              <input
                type="date"
                id="expirationDate"
                name="expirationDate"
                value={formData.expirationDate}
                onChange={handleChange}
                className="w-full px-4 py-2 border border-gray-300 rounded-lg focus:outline-none focus:ring-2 focus:ring-blue-500"
                required
              />
            </div>
          </div>

          <div className="grid grid-cols-2 gap-4">
            <div>
              <label htmlFor="contracts" className="block text-sm font-medium text-gray-700 mb-2">
                Contracts *
              </label>
              <input
                type="number"
                id="contracts"
                name="contracts"
                value={formData.contracts}
                onChange={handleChange}
                placeholder="e.g., 1"
                min="1"
                max={maxContracts}
                step="1"
                className="w-full px-4 py-2 border border-gray-300 rounded-lg focus:outline-none focus:ring-2 focus:ring-blue-500"
                required
              />
              <p className="mt-1 text-xs text-gray-500">1 contract = 100 shares</p>
            </div>

            <div>
              <label htmlFor="premiumPerContract" className="block text-sm font-medium text-gray-700 mb-2">
                Premium per Contract *
              </label>
              <input
                type="number"
                id="premiumPerContract"
                name="premiumPerContract"
                value={formData.premiumPerContract}
                onChange={handleChange}
                placeholder="e.g., 2.50"
                min="0.01"
                step="0.01"
                className="w-full px-4 py-2 border border-gray-300 rounded-lg focus:outline-none focus:ring-2 focus:ring-blue-500"
                required
              />
            </div>
          </div>

          {totalPremium > 0 && (
            <div className="bg-green-50 border border-green-200 rounded-lg p-4">
              <p className="text-sm font-medium text-green-900">
                Total Premium: ${totalPremium.toFixed(2)}
              </p>
            </div>
          )}

          <div>
            <label htmlFor="notes" className="block text-sm font-medium text-gray-700 mb-2">
              Notes (optional)
            </label>
            <textarea
              id="notes"
              name="notes"
              value={formData.notes}
              onChange={handleChange}
              placeholder="Add any notes about this trade..."
              rows={3}
              className="w-full px-4 py-2 border border-gray-300 rounded-lg focus:outline-none focus:ring-2 focus:ring-blue-500"
            />
          </div>

          <div className="flex space-x-4">
            <button
              type="submit"
              disabled={loading}
              className={`flex-1 px-6 py-3 rounded-lg font-medium transition ${
                loading
                  ? 'bg-gray-300 text-gray-500 cursor-not-allowed'
                  : 'bg-blue-600 text-white hover:bg-blue-700'
              }`}
            >
              {loading ? 'Creating...' : 'Sell Covered Call'}
            </button>
            
            <button
              type="button"
              onClick={() => navigate('/options')}
              className="px-6 py-3 border border-gray-300 rounded-lg font-medium text-gray-700 hover:bg-gray-50 transition"
            >
              Cancel
            </button>
          </div>
        </form>
      </div>
    </div>
  );
}
