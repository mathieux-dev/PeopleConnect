import React, { useState, useEffect } from 'react';
import { Users, Search, LogOut, Shield, User as UserIcon, ChevronLeft, ChevronRight, X, Plus } from 'lucide-react';
import toast from 'react-hot-toast';
import { useAuth } from '../../hooks/useAuth';
import { PersonResponseDto } from '../../types/api.types';
import { personsApi } from '../../services/api';
import { PersonCard } from '../../components/common/PersonCard';
import { PersonDetailsModal } from '../../components/modals/PersonDetailsModal';
import { AddPersonModal } from '../../components/modals/AddPersonModal';
import { EditPersonModal } from '../../components/modals/EditPersonModal';
import { PersonCardSkeletonGrid } from '../../components/common/PersonCardSkeleton';
import { ErrorState } from '../../components/common/ErrorState';

export const PersonsPage: React.FC = () => {
  const { user, logout, isAdmin } = useAuth();
  const [persons, setPersons] = useState<PersonResponseDto[]>([]);
  const [filteredPersons, setFilteredPersons] = useState<PersonResponseDto[]>([]);
  const [searchTerm, setSearchTerm] = useState('');
  const [selectedPerson, setSelectedPerson] = useState<PersonResponseDto | null>(null);
  const [isModalOpen, setIsModalOpen] = useState(false);
  const [isAddPersonModalOpen, setIsAddPersonModalOpen] = useState(false);
  const [isEditPersonModalOpen, setIsEditPersonModalOpen] = useState(false);
  const [isLoading, setIsLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  
  // Pagination states
  const [currentPage, setCurrentPage] = useState(1);
  const [itemsPerPage] = useState(5);
  const [paginatedPersons, setPaginatedPersons] = useState<PersonResponseDto[]>([]);

  useEffect(() => {
    loadPersons();
  }, []);

  useEffect(() => {
    if (searchTerm.trim() === '') {
      setFilteredPersons(persons);
    } else {
      const searchLower = searchTerm.toLowerCase();
      const searchDigits = searchTerm.replace(/\D/g, '');
      
      const filtered = persons.filter((person) => {
        const nameMatch = person.nome.toLowerCase().includes(searchLower);
        const cpfMatch = searchDigits.length > 0 && person.cpf.includes(searchDigits);
        const emailMatch = person.email?.toLowerCase().includes(searchLower);
        const naturalidadeMatch = person.naturalidade?.toLowerCase().includes(searchLower);
        const nacionalidadeMatch = person.nacionalidade?.toLowerCase().includes(searchLower);
        
        const contactMatch = person.contacts?.some(contact => 
          contact.value.toLowerCase().includes(searchLower) ||
          contact.type.toLowerCase().includes(searchLower)
        );
        
        return nameMatch || cpfMatch || emailMatch || naturalidadeMatch || nacionalidadeMatch || contactMatch;
      });
      
      setFilteredPersons(filtered);
    }
    setCurrentPage(1);
  }, [persons, searchTerm]);

  useEffect(() => {
    const startIndex = (currentPage - 1) * itemsPerPage;
    const endIndex = startIndex + itemsPerPage;
    const paginated = filteredPersons.slice(startIndex, endIndex);
    
    setPaginatedPersons(paginated);
  }, [filteredPersons, currentPage, itemsPerPage]);

  const loadPersons = async () => {
    try {
      setIsLoading(true);
      setError(null);
      const data = await personsApi.getAll();
      setPersons(data);
    } catch (error: any) {
      console.error('Error loading persons:', error);
      if (error.status === 401) {
        setError('Sessão expirada');
      } else if (error.status === 403) {
        setError('Você não tem permissão para visualizar as pessoas');
      } else if (error.message?.includes('Erro de conexão')) {
        setError('network');
      } else {
        setError('Erro ao carregar lista de pessoas');
      }
    } finally {
      setIsLoading(false);
    }
  };

  const handleViewDetails = async (person: PersonResponseDto) => {
    try {
      setSelectedPerson(person);
      setIsModalOpen(true);
    } catch (error: any) {
      console.error('Error opening person details:', error);
      toast.error('Erro ao abrir detalhes da pessoa');
    }
  };

  const handlePersonDeleted = (personId: string) => {
    setPersons(prev => prev.filter(p => p.id !== personId));
  };

  const handlePersonAdded = (newPerson: PersonResponseDto) => {
    setPersons(prev => [...prev, newPerson]);
  };

  const handlePersonEdit = (person: PersonResponseDto) => {
    setSelectedPerson(person);
    setIsModalOpen(false);
    setIsEditPersonModalOpen(true);
  };

  const handlePersonUpdated = (updatedPerson: PersonResponseDto) => {
    setPersons(prev => prev.map(p => p.id === updatedPerson.id ? updatedPerson : p));
    setIsEditPersonModalOpen(false);
    setSelectedPerson(null);
  };

  const handleLogout = () => {
    logout();
  };

  const totalPages = Math.ceil(filteredPersons.length / itemsPerPage);
  const startItem = (currentPage - 1) * itemsPerPage + 1;
  const endItem = Math.min(currentPage * itemsPerPage, filteredPersons.length);

  const getVisiblePages = () => {
    const maxVisiblePages = 5;
    let startPage = Math.max(1, currentPage - Math.floor(maxVisiblePages / 2));
    let endPage = Math.min(totalPages, startPage + maxVisiblePages - 1);
    
    if (endPage - startPage + 1 < maxVisiblePages) {
      startPage = Math.max(1, endPage - maxVisiblePages + 1);
    }
    
    return Array.from({ length: endPage - startPage + 1 }, (_, i) => startPage + i);
  };

  const handlePageChange = (page: number) => {
    setCurrentPage(page);
  };

  const handlePreviousPage = () => {
    if (currentPage > 1) {
      setCurrentPage(currentPage - 1);
    }
  };

  const handleNextPage = () => {
    if (currentPage < totalPages) {
      setCurrentPage(currentPage + 1);
    }
  };

  return (
    <div className="min-h-screen bg-gray-50">
      {/* Header */}
      <header className="bg-white shadow-sm border-b border-gray-200">
        <div className="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8">
          <div className="flex items-center justify-between h-16">
            {/* Logo and Title */}
            <div className="flex items-center gap-4">
              <div className="flex items-center justify-center w-10 h-10 bg-blue-600 rounded-full">
                <Users className="w-6 h-6 text-white" />
              </div>
              {/* Text hidden on mobile, visible on small screens and up */}
              <div className="hidden sm:block">
                <h1 className="text-xl font-bold text-gray-900">PeopleConnect</h1>
                <p className="text-sm text-gray-600">Sistema de Gerenciamento de Pessoas</p>
              </div>
            </div>

            {/* User Info and Actions */}
            <div className="flex items-center gap-4">
              <div className="flex items-center gap-2 p-2 rounded-lg">
                <div className={`flex items-center justify-center w-8 h-8 rounded-full ${
                  isAdmin ? 'bg-blue-100' : 'bg-gray-100'
                }`}>
                  {isAdmin ? (
                    <Shield className="w-4 h-4 text-blue-600" />
                  ) : (
                    <UserIcon className="w-4 h-4 text-gray-600" />
                  )}
                </div>
                {/* User details hidden on mobile, visible on small screens and up */}
                <div className="text-right hidden sm:block">
                  <p className="text-sm font-medium text-gray-900">{user?.username}</p>
                  <span className={`text-xs px-2 py-0.5 rounded-full font-medium ${
                    isAdmin 
                      ? 'bg-blue-100 text-blue-800' 
                      : 'bg-gray-100 text-gray-800'
                  }`}>
                    {isAdmin ? 'Admin' : 'Cliente'}
                  </span>
                </div>
              </div>
              <button
                onClick={handleLogout}
                className="flex items-center gap-2 px-3 py-2 text-gray-600 hover:text-gray-800 hover:bg-gray-100 rounded-lg transition-colors duration-200"
              >
                <LogOut className="w-4 h-4" />
                {/* Button text hidden on mobile, visible on small screens and up */}
                <span className="hidden sm:inline">Sair</span>
              </button>
            </div>
          </div>
        </div>
      </header>

      {/* Main Content */}
      <main className="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8 py-8">
        {/* Search Bar */}
        <div className="mb-8">
          <div className="relative max-w-md">
            <Search className="absolute left-3 top-1/2 transform -translate-y-1/2 w-5 h-5 text-gray-400" />
            <input
              type="text"
              placeholder="Buscar por nome, CPF, email, contato..."
              value={searchTerm}
              onChange={(e) => setSearchTerm(e.target.value)}
              className="w-full pl-10 pr-10 py-3 border border-gray-300 rounded-lg focus:outline-none focus:ring-2 focus:ring-blue-500 focus:border-transparent transition-all duration-200"
            />
            {searchTerm && (
              <button
                onClick={() => setSearchTerm('')}
                className="absolute right-3 top-1/2 transform -translate-y-1/2 w-5 h-5 text-gray-400 hover:text-gray-600 transition-colors"
              >
                <X className="w-5 h-5" />
              </button>
            )}
          </div>
          {searchTerm && (
            <p className="mt-2 text-sm text-gray-600">
              {filteredPersons.length} resultado(s) encontrado(s) para "{searchTerm}"
            </p>
          )}
        </div>

        {/* Stats and Actions */}
        <div className="grid grid-cols-1 md:grid-cols-2 gap-6 mb-8">
          <div className="bg-white rounded-xl shadow-sm p-6 border border-gray-200">
            <div className="flex items-center gap-3">
              <div className="w-10 h-10 bg-blue-100 rounded-full flex items-center justify-center">
                <Users className="w-5 h-5 text-blue-600" />
              </div>
              <div>
                <p className="text-2xl font-bold text-gray-900">{persons.length}</p>
                <p className="text-sm text-gray-600">Total de pessoas</p>
              </div>
            </div>
          </div>
          {isAdmin && (
            <div 
              onClick={() => setIsAddPersonModalOpen(true)}
              className="bg-white rounded-xl shadow-sm p-6 border border-gray-200 cursor-pointer hover:bg-gray-50 transition-colors group"
            >
              <div className="flex items-center gap-3">
                <div className="w-10 h-10 bg-green-100 rounded-full flex items-center justify-center group-hover:bg-green-200 transition-colors">
                  <Plus className="w-5 h-5 text-green-600" />
                </div>
                <div>
                  <p className="text-lg font-medium text-gray-900">Adicionar Pessoa</p>
                  <p className="text-sm text-gray-600">Cadastrar nova pessoa</p>
                </div>
              </div>
            </div>
          )}
        </div>

        {/* Persons Grid */}
        {isLoading ? (
          <PersonCardSkeletonGrid count={6} />
        ) : error ? (
          <ErrorState
            type={error === 'network' ? 'network' : 'error'}
            title={error === 'network' ? undefined : 'Erro ao Carregar'}
            message={error === 'network' ? undefined : error}
            onRetry={loadPersons}
          />
        ) : filteredPersons.length === 0 ? (
          <div className="text-center py-12">
            <div className="w-16 h-16 bg-gray-100 rounded-full flex items-center justify-center mx-auto mb-4">
              <Users className="w-8 h-8 text-gray-400" />
            </div>
            <h3 className="text-lg font-medium text-gray-900 mb-2">
              {searchTerm ? 'Nenhuma pessoa encontrada' : 'Nenhuma pessoa cadastrada'}
            </h3>
            <p className="text-gray-600">
              {searchTerm 
                ? 'Tente ajustar os termos da sua busca.' 
                : 'Quando pessoas forem cadastradas, elas aparecerão aqui.'
              }
            </p>
          </div>
        ) : (
          <>
            <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-6">
              {paginatedPersons.map((person) => (
                <PersonCard
                  key={person.id}
                  person={person}
                  onViewDetails={handleViewDetails}
                />
              ))}
            </div>

            {/* Pagination Controls */}
            {totalPages > 1 && (
              <div className="mt-8 flex items-center justify-between">
                <div className="text-sm text-gray-700">
                  {/* Versão completa para desktop */}
                  <span className="hidden sm:block">
                    Mostrando <span className="font-medium">{startItem}</span> a{' '}
                    <span className="font-medium">{endItem}</span> de{' '}
                    <span className="font-medium">{filteredPersons.length}</span> resultados
                  </span>
                  {/* Versão compacta para mobile */}
                  <span className="sm:hidden">
                    <span className="font-medium">{startItem}-{endItem}</span> de{' '}
                    <span className="font-medium">{filteredPersons.length}</span>
                  </span>
                </div>
                
                <nav className="flex items-center gap-2">
                  <button
                    onClick={handlePreviousPage}
                    disabled={currentPage === 1}
                    className="flex items-center gap-1 px-3 py-2 text-sm font-medium text-gray-500 hover:text-gray-700 disabled:text-gray-300 disabled:cursor-not-allowed"
                  >
                    <ChevronLeft className="w-4 h-4" />
                    <span className="hidden sm:inline">Anterior</span>
                  </button>
                  
                  <div className="flex gap-1">
                    {getVisiblePages().map((page) => (
                      <button
                        key={page}
                        onClick={() => handlePageChange(page)}
                        className={`px-3 py-2 text-sm font-medium rounded-md transition-colors ${
                          page === currentPage
                            ? 'bg-blue-600 text-white'
                            : 'text-gray-700 hover:bg-gray-100'
                        }`}
                      >
                        {page}
                      </button>
                    ))}
                  </div>
                  
                  <button
                    onClick={handleNextPage}
                    disabled={currentPage === totalPages}
                    className="flex items-center gap-1 px-3 py-2 text-sm font-medium text-gray-500 hover:text-gray-700 disabled:text-gray-300 disabled:cursor-not-allowed"
                  >
                    <span className="hidden sm:inline">Próximo</span>
                    <ChevronRight className="w-4 h-4" />
                  </button>
                </nav>
              </div>
            )}
          </>
        )}
      </main>

      {/* Person Details Modal */}
      <PersonDetailsModal
        person={selectedPerson}
        isOpen={isModalOpen}
        onClose={() => {
          setIsModalOpen(false);
          setSelectedPerson(null);
        }}
        onPersonDeleted={handlePersonDeleted}
        onPersonEdit={handlePersonEdit}
      />

      {/* Add Person Modal */}
      {isAdmin && (
        <AddPersonModal
          isOpen={isAddPersonModalOpen}
          onClose={() => setIsAddPersonModalOpen(false)}
          onPersonAdded={handlePersonAdded}
        />
      )}

      {/* Edit Person Modal */}
      <EditPersonModal
        person={selectedPerson}
        isOpen={isEditPersonModalOpen}
        onClose={() => {
          setIsEditPersonModalOpen(false);
          setSelectedPerson(null);
        }}
        onPersonUpdated={handlePersonUpdated}
      />
    </div>
  );
};
