import { useState } from 'react';
import { useNavigate } from 'react-router-dom';
import { optionsApi } from '../api/client';

export default function AddCashSecuredPut() {
  const navigate = useNavigate();
  const [formData, setFormData] = useState({
    underlyingSymbol: '',
    strikePrice: '',
    expirationDate: '',
    contracts: '',
    premiumPerContract: '',
    account: '',
    notes: ''
  });
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    
    if (!formData.underlyingSymbol || !formData.strikePrice || !formData.expirationDate || 
        !formData.contracts || !formData.premiumPerContract || !formData.account) {
      setError('All required fields must be filled');
      return;
    }

    try {
      setLoading(true);
      setError(null);
      
      await optionsApi.createCashSecuredPut({
        underlyingSymbol: formData.underlyingSymbol.toUpperCase(),
        strikePrice: parseFloat(formData.strikePrice),
        expirationDate: formData.expirationDate,
        contracts: parseInt(formData.contracts),
        premiumPerContract: parseFloat(formData.premiumPerContract),
        account: formData.account,
        notes: formData.notes || undefined
      });

      navigate('/options');
    } catch (err: any) {
      setError(err.response?.data || 'Failed to create cash-secured put');
    } finally {
      setLoading(false);
    }
  };

  const handleChange = (e: React.ChangeEvent<HTMLInputElement | HTMLTextAreaElement>) => {
    setFormData({
      ...formData,
      [e.target.name]: e.target.value
    });
  };

  const requiredCapital = formData.contracts && formData.strikePrice 
    ? parseInt(formData.contracts) * parseFloat(formData.strikePrice) * 100 
    : 0;
  
  const totalPremium = formData.contracts && formData.premiumPerContract 
    ? parseInt(formData.contracts) * parseFloat(formData.premiumPerContract) * 100 
    : 0;

  return (
    <div className="space-y-6">
      <div>
        <h1 className="text-3xl font-bold text-gray-900">Sell Cash-Secured Put</h1>
        <p className="mt-2 text-gray-600">Sell put options secured by cash reserves</p>
      </div>

      <div className="bg-white rounded-lg shadow p-6 max-w-2xl">
        <form onSubmit={handleSubmit} className="space-y-6">
          {error && (
            <div className="bg-red-50 border border-red-200 rounded-lg p-4">
              <p className="text-red-800 text-sm">{error}</p>
            </div>
          )}

          <div>
            <label htmlFor="underlyingSymbol" className="block text-sm font-medium text-gray-700 mb-2">
              Underlying Symbol *
            </label>
            <input
              type="text"
              id="underlyingSymbol"
              name="underlyingSymbol"
              value={formData.underlyingSymbol}
              onChange={handleChange}
              placeholder="e.g., AAPL"
              className="w-full px-4 py-2 border border-gray-300 rounded-lg focus:outline-none focus:ring-2 focus:ring-blue-500 uppercase"
              required
            />
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
                placeholder="e.g., 145.00"
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
                placeholder="e.g., 3.00"
                min="0.01"
                step="0.01"
                className="w-full px-4 py-2 border border-gray-300 rounded-lg focus:outline-none focus:ring-2 focus:ring-blue-500"
                required
              />
            </div>
          </div>

          <div>
            <label htmlFor="account" className="block text-sm font-medium text-gray-700 mb-2">
              Account Name *
            </label>
            <input
              type="text"
              id="account"
              name="account"
              value={formData.account}
              onChange={handleChange}
              placeholder="e.g., My Brokerage Account"
              className="w-full px-4 py-2 border border-gray-300 rounded-lg focus:outline-none focus:ring-2 focus:ring-blue-500"
              required
            />
          </div>

          {(requiredCapital > 0 || totalPremium > 0) && (
            <div className="bg-purple-50 border border-purple-200 rounded-lg p-4 space-y-2">
              {requiredCapital > 0 && (
                <p className="text-sm font-medium text-purple-900">
                  Required Capital: ${requiredCapital.toFixed(2)}
                </p>
              )}
              {totalPremium > 0 && (
                <p className="text-sm font-medium text-green-900">
                  Premium Collected: ${totalPremium.toFixed(2)}
                </p>
              )}
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
                  : 'bg-purple-600 text-white hover:bg-purple-700'
              }`}
            >
              {loading ? 'Creating...' : 'Sell Cash-Secured Put'}
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

      <div className="bg-blue-50 border border-blue-200 rounded-lg p-4 max-w-2xl">
        <h3 className="font-medium text-blue-900 mb-2">💡 About Cash-Secured Puts</h3>
        <p className="text-sm text-blue-800">
          When you sell a cash-secured put, you're agreeing to buy the stock at the strike price if assigned. 
          Make sure you have enough cash in your account to cover the purchase.
        </p>
      </div>
    </div>
  );
}
