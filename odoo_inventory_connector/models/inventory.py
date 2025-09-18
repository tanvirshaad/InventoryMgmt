# -*- coding: utf-8 -*-

from odoo import api, fields, models, _
from odoo.exceptions import UserError, ValidationError
import requests
import json
import logging
from datetime import datetime
import re

_logger = logging.getLogger(__name__)

class InventoryConnectorInventory(models.Model):
    _name = 'inventory.connector.inventory'
    _description = 'External Inventory'
    _inherit = ['mail.thread', 'mail.activity.mixin']
    _order = 'create_date desc'

    name = fields.Char('Title', required=True, tracking=True)
    description = fields.Text('Description')
    external_id = fields.Integer('External ID', readonly=True)
    api_token = fields.Char('API Token', required=True)
    api_url = fields.Char('API URL', required=True, default='https://localhost:5001')
    category = fields.Char('Category')
    is_public = fields.Boolean('Is Public')
    created_at = fields.Datetime('Created At')
    updated_at = fields.Datetime('Updated At')
    item_count = fields.Integer('Item Count', readonly=True)
    last_sync = fields.Datetime('Last Synchronized', readonly=True)
    active = fields.Boolean('Active', default=True)
    
    # Relations
    field_definition_ids = fields.One2many('inventory.connector.field.definition', 'inventory_id', string='Field Definitions')
    field_aggregation_ids = fields.One2many('inventory.connector.field.aggregation', 'inventory_id', string='Field Aggregations')
    item_ids = fields.One2many('inventory.connector.item', 'inventory_id', string='Items')
    tag_ids = fields.One2many('inventory.connector.tag', 'inventory_id', string='Tags')
    
    _sql_constraints = [
        ('api_token_unique', 'UNIQUE(api_token)', 'API Token must be unique')
    ]
    
    @api.constrains('api_token')
    def _check_api_token(self):
        for record in self:
            if not record.api_token:
                raise ValidationError(_("API Token cannot be empty"))
    
    def _parse_datetime(self, datetime_str):
        """Parse datetime string from API response"""
        if not datetime_str:
            return None
        
        try:
            # Remove timezone info and extra precision if present
            # Handle formats like: 2025-08-30T17:59:37.85758, 2025-08-30T17:59:37.857Z, etc.
            clean_str = datetime_str
            
            # Remove 'Z' suffix if present
            if clean_str.endswith('Z'):
                clean_str = clean_str[:-1]
            
            # Handle microseconds - limit to 6 digits
            if '.' in clean_str:
                date_part, microsecond_part = clean_str.split('.')
                # Limit microseconds to 6 digits and pad if necessary
                microsecond_part = microsecond_part[:6].ljust(6, '0')
                clean_str = f"{date_part}.{microsecond_part}"
            
            # Try different datetime formats
            formats = [
                '%Y-%m-%dT%H:%M:%S.%f',  # With microseconds
                '%Y-%m-%dT%H:%M:%S',     # Without microseconds
                '%Y-%m-%d %H:%M:%S.%f',  # Space separated with microseconds
                '%Y-%m-%d %H:%M:%S',     # Space separated without microseconds
            ]
            
            for fmt in formats:
                try:
                    return datetime.strptime(clean_str, fmt)
                except ValueError:
                    continue
            
            # If none of the formats work, log the issue and return None
            _logger.warning("Could not parse datetime string: %s", datetime_str)
            return None
            
        except Exception as e:
            _logger.error("Error parsing datetime '%s': %s", datetime_str, str(e))
            return None
                
    def action_test_connection(self):
        """Test the connection to the API server"""
        self.ensure_one()
        
        try:
            # Force HTTPS if port 5001 is detected (typical .NET HTTPS port)
            base_url = self.api_url.rstrip('/')
            if ':5001' in base_url and not base_url.startswith('https'):
                base_url = 'https://localhost:5001'
                
            message = f"Testing connection to API...\n"
            message += f"Base URL: {base_url}\n\n"
            
            # Based on your InventoryApiController, test the actual endpoints
            test_endpoints = [
                "/api/InventoryApi/info",
                "/api/InventoryApi/aggregated", 
                "/api/InventoryApi/items",
                "/swagger",
                "/swagger/index.html"
            ]
            
            for endpoint in test_endpoints:
                test_url = f"{base_url}{endpoint}"
                message += f"Trying endpoint: {test_url}\n"
                try:
                    # First try without token
                    response = requests.get(test_url, timeout=10, verify=False)
                    message += f"  Response (no auth): Status {response.status_code}\n"
                    if response.status_code == 400 and 'token' in response.text.lower():
                        message += f"  API expects token parameter\n"
                    elif response.status_code == 200:
                        message += f"  Content: {response.text[:150]}...\n"
                        message += f"  SUCCESS! This endpoint works without authentication.\n"
                    
                    # Now try with token as query parameter
                    if self.api_token:
                        response = requests.get(test_url, params={'token': self.api_token}, timeout=10, verify=False)
                        message += f"  Response (with token): Status {response.status_code}\n"
                        if response.status_code == 200:
                            message += f"  Content: {response.text[:150]}...\n"
                            message += f"  SUCCESS! This endpoint works with token.\n"
                        elif response.status_code == 401:
                            message += f"  Got 401 - token invalid or user not authorized\n"
                        elif response.status_code == 400:
                            message += f"  Got 400 - bad request, check token format\n"
                    
                except Exception as e:
                    message += f"  Error: {str(e)}\n"
                message += "\n"
            
            # Now try some paths with authentication
            message += f"\nTrying paths with authentication token...\n"
            auth_endpoints = [
                "/api/inventory/info",
                "/api/InventoryApi/info",
                "/info", 
                "/api/Info",
                "/api/Inventory"
            ]
            
            for endpoint in auth_endpoints:
                test_url = f"{base_url}{endpoint}"
                message += f"Trying endpoint with token: {test_url}\n"
                
                # Try as query parameter
                try:
                    response = requests.get(test_url, params={'token': self.api_token}, timeout=10, verify=False)
                    message += f"  Query param auth - Status: {response.status_code}\n"
                    if response.status_code == 200:
                        message += f"  Content: {response.text[:150]}...\n"
                        message += f"  SUCCESS! This endpoint works with token as query parameter.\n"
                    elif response.status_code == 401:
                        message += f"  Got 401 - endpoint exists but token invalid/required\n"
                    elif response.status_code == 404:
                        message += f"  Got 404 - endpoint not found\n"
                except Exception as e:
                    message += f"  Query param error: {str(e)}\n"
                    
                # Try as Bearer token
                try:
                    headers = {'Authorization': f'Bearer {self.api_token}'}
                    response = requests.get(test_url, headers=headers, timeout=10, verify=False)
                    message += f"  Bearer token auth - Status: {response.status_code}\n"
                    if response.status_code == 200:
                        message += f"  Content: {response.text[:150]}...\n"
                        message += f"  SUCCESS! This endpoint works with Bearer token.\n"
                    elif response.status_code == 401:
                        message += f"  Got 401 - endpoint exists but token invalid/required\n"
                    elif response.status_code == 404:
                        message += f"  Got 404 - endpoint not found\n"
                except Exception as e:
                    message += f"  Bearer token error: {str(e)}\n"
                
                message += "\n"
                
            # Return diagnostic information
            return {
                'type': 'ir.actions.client',
                'tag': 'display_notification',
                'params': {
                    'title': _('Connection Test Results'),
                    'message': message,
                    'sticky': True,
                    'type': 'info',
                }
            }
        except Exception as e:
            _logger.error("Error in test connection: %s", str(e))
            return {
                'type': 'ir.actions.client',
                'tag': 'display_notification',
                'params': {
                    'title': _('Connection Test Error'),
                    'message': str(e),
                    'sticky': True,
                    'type': 'danger',
                }
            }
                
    def action_sync_inventory(self):
        """Synchronize inventory data from external API"""
        self.ensure_one()
        
        try:
            # First, get basic inventory info
            # Ensure the API URL is properly formatted (doesn't end with a slash)
            base_url = self.api_url.rstrip('/')
            
            # Force HTTPS if port 5001 is detected (typical .NET HTTPS port)
            if ':5001' in base_url and not base_url.startswith('https'):
                base_url = 'https://localhost:5001'
                # Also update the stored URL
                self.api_url = base_url
            
            # Based on the InventoryApiController, use the correct endpoint
            info_url = f"{base_url}/api/InventoryApi/info"
            
            _logger.info("Attempting to connect to API: %s with token: %s", info_url, self.api_token)
            
            # Try the API endpoint with token as query parameter
            try:
                response = requests.get(info_url, params={'token': self.api_token}, timeout=10, verify=False)
                _logger.info("API response status: %s", response.status_code)
                
                if response.status_code != 200:
                    error_msg = f"Failed to connect to API: Status {response.status_code}"
                    if hasattr(response, 'text'):
                        error_msg += f" - {response.text or 'No response content'}"
                    raise UserError(_(error_msg))
                    
            except requests.exceptions.RequestException as e:
                _logger.error("Request exception: %s", str(e))
                raise UserError(_("Connection error: %s - Please check if the API server is running and accessible") % str(e))
                
            try:
                info_data = response.json()
                _logger.info("Parsed info data: %s", info_data)
            except Exception as e:
                _logger.error("Failed to parse JSON: %s", str(e))
                _logger.error("Response content: %s", response.text[:500])
                raise UserError(_("Failed to parse API response: %s") % str(e))
            
            # Update basic inventory information - handle different possible field names
            update_values = {
                'last_sync': fields.Datetime.now(),
            }
            
            # Try different possible field names based on common API naming conventions
            if 'title' in info_data:
                update_values['name'] = info_data.get('title')
            elif 'name' in info_data:
                update_values['name'] = info_data.get('name')
                
            if 'description' in info_data:
                update_values['description'] = info_data.get('description')
                
            if 'id' in info_data:
                update_values['external_id'] = info_data.get('id')
            elif 'inventoryId' in info_data:
                update_values['external_id'] = info_data.get('inventoryId')
                
            if 'category' in info_data:
                update_values['category'] = info_data.get('category')
                
            if 'isPublic' in info_data:
                update_values['is_public'] = info_data.get('isPublic')
            elif 'public' in info_data:
                update_values['is_public'] = info_data.get('public')
                
            if 'createdAt' in info_data:
                update_values['created_at'] = self._parse_datetime(info_data.get('createdAt'))
            elif 'createDate' in info_data:
                update_values['created_at'] = self._parse_datetime(info_data.get('createDate'))
            elif 'creationDate' in info_data:
                update_values['created_at'] = self._parse_datetime(info_data.get('creationDate'))
                
            if 'updatedAt' in info_data:
                update_values['updated_at'] = self._parse_datetime(info_data.get('updatedAt'))
            elif 'updateDate' in info_data:
                update_values['updated_at'] = self._parse_datetime(info_data.get('updateDate'))
            elif 'lastModified' in info_data:
                update_values['updated_at'] = self._parse_datetime(info_data.get('lastModified'))
            
            self.write(update_values)
            
            # Then, get aggregated data - use the correct endpoint
            aggregated_url = f"{base_url}/api/InventoryApi/aggregated"
            
            _logger.info("Trying aggregated data endpoint: %s", aggregated_url)
            try:
                response = requests.get(aggregated_url, params={'token': self.api_token}, timeout=10, verify=False)
                
                if response.status_code != 200:
                    # If we couldn't get aggregated data, just skip this part
                    _logger.warning("Could not get aggregated data: Status %s - %s", response.status_code, response.text)
                else:
                    aggregated_data = response.json()
                    _logger.info("Aggregated data received: %s", json.dumps(aggregated_data, indent=2))
                    
                    # Update item count
                    self.item_count = aggregated_data.get('itemCount', 0)
                    
                    # Debug the entire aggregated_data structure
                    _logger.info("Full aggregated data: %s", json.dumps(aggregated_data, indent=2))
                    
                    # Try both PascalCase and camelCase field names (for .NET / JSON conventions)
                    custom_fields = None
                    if 'customFields' in aggregated_data:
                        custom_fields = aggregated_data.get('customFields')
                        _logger.info("Found customFields (camelCase): %s", len(custom_fields))
                    elif 'CustomFields' in aggregated_data:
                        custom_fields = aggregated_data.get('CustomFields')
                        _logger.info("Found CustomFields (PascalCase): %s", len(custom_fields))
                    else:
                        # Try to find the key case-insensitive
                        for key in aggregated_data.keys():
                            if key.lower() == 'customfields':
                                custom_fields = aggregated_data.get(key)
                                _logger.info("Found custom fields with key: %s", key)
                                break
                    
                    _logger.info("Custom fields data: %s", json.dumps(custom_fields, indent=2) if custom_fields else "None")
                    if not custom_fields:
                        _logger.warning("No custom fields found! Available keys: %s", list(aggregated_data.keys()))
                    
                    # Process custom fields if found
                    if custom_fields:
                        self._process_custom_fields(custom_fields)
                    else:
                        _logger.error("No custom fields found in API response. Please check the API implementation.")
                    
                    # Process field aggregations - try both naming conventions
                    aggregated_results = None
                    if 'aggregatedResults' in aggregated_data:
                        aggregated_results = aggregated_data.get('aggregatedResults')
                        _logger.info("Found aggregatedResults (camelCase): %s", len(aggregated_results))
                    elif 'AggregatedResults' in aggregated_data:
                        aggregated_results = aggregated_data.get('AggregatedResults')
                        _logger.info("Found AggregatedResults (PascalCase): %s", len(aggregated_results))
                    else:
                        # Try to find the key case-insensitive
                        for key in aggregated_data.keys():
                            if key.lower() == 'aggregatedresults':
                                aggregated_results = aggregated_data.get(key)
                                _logger.info("Found aggregated results with key: %s", key)
                                break
                                
                    _logger.info("Aggregated results data: %s", json.dumps(aggregated_results, indent=2) if aggregated_results else "None")
                    if not aggregated_results:
                        _logger.warning("No aggregated results found! Available keys: %s", list(aggregated_data.keys()))
                    
                    # Process field aggregations if found
                    if aggregated_results:
                        self._process_field_aggregations(aggregated_results)
                    else:
                        _logger.error("No aggregated results found in API response. Please check the API implementation.")
                    
            except Exception as e:
                _logger.warning("Error getting aggregated data: %s", str(e))
                # Continue anyway with the basic information
            
            return {
                'type': 'ir.actions.client',
                'tag': 'display_notification',
                'params': {
                    'title': _('Success'),
                    'message': _('Inventory data successfully synchronized'),
                    'sticky': False,
                    'type': 'success',
                }
            }
            
        except Exception as e:
            _logger.error("Error synchronizing inventory: %s", str(e))
            raise UserError(_("Error synchronizing inventory: %s") % str(e))
    
    def _process_custom_fields(self, custom_fields):
        """Process and update custom field definitions"""
        # Remove old field definitions
        self.field_definition_ids.unlink()
        
        _logger.info(f"Processing {len(custom_fields)} custom field definitions")
        
        # Create new field definitions
        for field_def in custom_fields:
            # Log the field definition for debugging
            _logger.info(f"Processing field definition: {field_def}")
            
            # Handle both PascalCase and camelCase field names
            name = field_def.get('name', field_def.get('Name', ''))
            field_type = field_def.get('type', field_def.get('Type', 'text'))
            description = field_def.get('description', field_def.get('Description', ''))
            show_in_table = field_def.get('showInTable', field_def.get('ShowInTable', False))
            
            # Get numeric config with case insensitivity
            numeric_config = field_def.get('numericConfig', field_def.get('NumericConfig', {}))
            min_value = 0
            max_value = 0
            is_integer = False
            
            if field_type == 'numeric' and numeric_config:
                if numeric_config is not None:  # Ensure numeric_config is not None
                    min_value = numeric_config.get('minValue', numeric_config.get('MinValue', 0))
                    max_value = numeric_config.get('maxValue', numeric_config.get('MaxValue', 0))
                    is_integer = numeric_config.get('isInteger', numeric_config.get('IsInteger', False))
                
            _logger.info(f"Creating field definition: {name} (type: {field_type}, show_in_table: {show_in_table})")
            
            try:
                field_def_id = self.env['inventory.connector.field.definition'].create({
                    'inventory_id': self.id,
                    'name': name,
                    'field_type': field_type,
                    'description': description,
                    'show_in_table': show_in_table,
                    'min_value': min_value,
                    'max_value': max_value,
                    'is_integer': is_integer,
                })
                _logger.info(f"Successfully created field definition with ID: {field_def_id.id}")
            except Exception as e:
                _logger.error(f"Failed to create field definition '{name}': {str(e)}")
                # Try again with default values
                try:
                    field_def_id = self.env['inventory.connector.field.definition'].create({
                        'inventory_id': self.id,
                        'name': name or 'Unnamed Field',
                        'field_type': field_type or 'text',
                        'description': description or '',
                        'show_in_table': show_in_table or False,
                        'min_value': 0,
                        'max_value': 0,
                        'is_integer': False,
                    })
                    _logger.info(f"Created field definition with fallback values, ID: {field_def_id.id}")
                except Exception as e2:
                    _logger.error(f"Failed again to create field definition: {str(e2)}")
    
    def _process_field_aggregations(self, aggregated_results):
        """Process and update field aggregation data"""
        # Remove old aggregations
        self.field_aggregation_ids.unlink()
        
        _logger.info(f"Processing {len(aggregated_results)} field aggregations")
        
        # Create new aggregations
        for agg in aggregated_results:
            # Log the aggregation for debugging
            _logger.info(f"Processing aggregation: {agg}")
            
            # Handle both PascalCase and camelCase field names
            field_name = agg.get('fieldName', agg.get('FieldName', ''))
            field_type = agg.get('fieldType', agg.get('FieldType', 'text'))
            
            if not field_name:
                _logger.warning(f"Skipping aggregation with no field name: {agg}")
                continue
                
            values = {
                'inventory_id': self.id,
                'field_name': field_name,
                'field_type': field_type,
            }
            
            # Add type-specific values
            if field_type == 'numeric':
                values.update({
                    'min_value': agg.get('minValue', agg.get('MinValue', 0)),
                    'max_value': agg.get('maxValue', agg.get('MaxValue', 0)),
                    'average_value': agg.get('averageValue', agg.get('AverageValue', 0)),
                    'median_value': agg.get('medianValue', agg.get('MedianValue', 0)),
                })
            elif field_type == 'boolean':
                values.update({
                    'true_count': agg.get('trueCount', agg.get('TrueCount', 0)),
                    'false_count': agg.get('falseCount', agg.get('FalseCount', 0)),
                    'true_percentage': agg.get('truePercentage', agg.get('TruePercentage', 0)),
                })
            elif field_type in ['text', 'multiline']:
                # Store most common values as JSON
                most_common_values = agg.get('mostCommonValues', agg.get('MostCommonValues', []))
                if most_common_values:
                    values['common_values_json'] = json.dumps(most_common_values)
            
            _logger.info(f"Creating field aggregation: {field_name} (type: {field_type})")
            
            try:
                agg_id = self.env['inventory.connector.field.aggregation'].create(values)
                _logger.info(f"Successfully created field aggregation with ID: {agg_id.id}")
            except Exception as e:
                _logger.error(f"Failed to create field aggregation '{field_name}': {str(e)}")
                # Try again with minimal values
                try:
                    minimal_values = {
                        'inventory_id': self.id,
                        'field_name': field_name or 'Unnamed Field',
                        'field_type': field_type or 'text',
                    }
                    agg_id = self.env['inventory.connector.field.aggregation'].create(minimal_values)
                    _logger.info(f"Created field aggregation with minimal values, ID: {agg_id.id}")
                except Exception as e2:
                    _logger.error(f"Failed again to create field aggregation: {str(e2)}")
            
    def action_import_items(self):
        """Import inventory items from external API"""
        self.ensure_one()
        
        try:
            # Get items from API
            base_url = self.api_url.rstrip('/')
            # Force HTTPS if port 5001 is detected
            if ':5001' in base_url and not base_url.startswith('https'):
                base_url = 'https://localhost:5001'
                
            items_url = f"{base_url}/api/InventoryApi/items"
            response = requests.get(items_url, params={'token': self.api_token}, timeout=30, verify=False)
            
            if response.status_code != 200:
                raise UserError(_("Failed to get items from API: %s") % response.text)
                
            items_data = response.json()
            _logger.info("Items data structure: %s", json.dumps(items_data[:2] if items_data else [], indent=2))  # Log first 2 items
            
            # Process each item
            imported_count = 0
            updated_count = 0
            now = fields.Datetime.now()
            
            for item_data in items_data:
                # Check if item already exists
                external_id = str(item_data.get('id'))
                existing_item = self.env['inventory.connector.item'].search([
                    ('inventory_id', '=', self.id),
                    ('external_id', '=', external_id)
                ], limit=1)
                
                item_values = {
                    'name': item_data.get('name', f"Item {external_id}"),
                    'inventory_id': self.id,
                    'external_id': external_id,
                    'last_update': now,
                }
                
                if existing_item:
                    # Update existing item
                    existing_item.write(item_values)
                    
                    # Clear existing field values
                    existing_item.field_value_ids.unlink()
                    updated_count += 1
                else:
                    # Create new item
                    item_values['import_date'] = now
                    item = self.env['inventory.connector.item'].create(item_values)
                    existing_item = item
                    imported_count += 1
                
                # Process custom field values
                # First check if we have a customFields dictionary
                processed_fields = {}
                
                if item_data.get('customFields'):
                    _logger.info(f"Processing customFields dictionary for item {existing_item.name}")
                    for field_name, field_value in item_data.get('customFields').items():
                        # Skip empty values
                        if field_value is None or field_value == '':
                            continue
                            
                        # Find field definition
                        field_def = self.field_definition_ids.filtered(lambda fd: fd.name == field_name)
                        if not field_def:
                            _logger.warning(f"No field definition found for field '{field_name}'")
                            continue
                            
                        # Create field value
                        field_value_data = {
                            'item_id': existing_item.id,
                            'field_name': field_name,
                            'field_type': field_def.field_type,
                        }
                        
                        # Set type-specific value
                        if field_def.field_type == 'numeric':
                            field_value_data['numeric_value'] = float(field_value)
                        elif field_def.field_type == 'boolean':
                            field_value_data['boolean_value'] = bool(field_value)
                        else:
                            field_value_data['text_value'] = str(field_value)
                            
                        self.env['inventory.connector.field.value'].create(field_value_data)
                        processed_fields[field_name] = True
                
                # Now check for individual field properties in the item data
                _logger.info(f"Checking for individual field properties for item {existing_item.name}")
                
                # Process text fields
                for i in range(1, 4):  # 1 to 3
                    field_key = f"textField{i}Value"
                    if field_key in item_data and item_data[field_key] is not None:
                        field_name_key = f"textField{i}Name"
                        field_name = None
                        
                        # Try to get the field name from field definitions
                        for field_def in self.field_definition_ids:
                            if field_def.field_type == 'text' and not processed_fields.get(field_def.name):
                                field_name = field_def.name
                                processed_fields[field_name] = True
                                break
                        
                        if not field_name:
                            # If we don't find a matching field definition, use a default name
                            field_name = f"Text Field {i}"
                            
                        _logger.info(f"Creating text field value '{field_name}' = '{item_data[field_key]}'")
                        
                        self.env['inventory.connector.field.value'].create({
                            'item_id': existing_item.id,
                            'field_name': field_name,
                            'field_type': 'text',
                            'text_value': str(item_data[field_key]),
                        })
                
                # Process numeric fields
                for i in range(1, 4):  # 1 to 3
                    field_key = f"numericField{i}Value"
                    if field_key in item_data and item_data[field_key] is not None:
                        field_name_key = f"numericField{i}Name"
                        field_name = None
                        
                        # Try to get the field name from field definitions
                        for field_def in self.field_definition_ids:
                            if field_def.field_type == 'numeric' and not processed_fields.get(field_def.name):
                                field_name = field_def.name
                                processed_fields[field_name] = True
                                break
                        
                        if not field_name:
                            # If we don't find a matching field definition, use a default name
                            field_name = f"Numeric Field {i}"
                            
                        _logger.info(f"Creating numeric field value '{field_name}' = {item_data[field_key]}")
                        
                        self.env['inventory.connector.field.value'].create({
                            'item_id': existing_item.id,
                            'field_name': field_name,
                            'field_type': 'numeric',
                            'numeric_value': float(item_data[field_key]),
                        })
                
                # Process boolean fields
                for i in range(1, 4):  # 1 to 3
                    field_key = f"booleanField{i}Value"
                    if field_key in item_data and item_data[field_key] is not None:
                        field_name_key = f"booleanField{i}Name"
                        field_name = None
                        
                        # Try to get the field name from field definitions
                        for field_def in self.field_definition_ids:
                            if field_def.field_type == 'boolean' and not processed_fields.get(field_def.name):
                                field_name = field_def.name
                                processed_fields[field_name] = True
                                break
                        
                        if not field_name:
                            # If we don't find a matching field definition, use a default name
                            field_name = f"Boolean Field {i}"
                            
                        _logger.info(f"Creating boolean field value '{field_name}' = {item_data[field_key]}")
                        
                        self.env['inventory.connector.field.value'].create({
                            'item_id': existing_item.id,
                            'field_name': field_name,
                            'field_type': 'boolean',
                            'boolean_value': bool(item_data[field_key]),
                        })
                
                # Process tags
                tag_ids = []
                
                # Check for tags array
                if item_data.get('tags'):
                    _logger.info(f"Processing tags array for item {existing_item.name}")
                    for tag_name in item_data.get('tags'):
                        tag = self.env['inventory.connector.tag'].search([
                            ('inventory_id', '=', self.id),
                            ('name', '=', tag_name)
                        ], limit=1)
                        
                        if not tag:
                            tag = self.env['inventory.connector.tag'].create({
                                'name': tag_name,
                                'inventory_id': self.id,
                            })
                            _logger.info(f"Created new tag: {tag_name}")
                            
                        tag_ids.append(tag.id)
                
                # Check for comma-separated tag string
                elif item_data.get('tagsString'):
                    _logger.info(f"Processing tags string for item {existing_item.name}")
                    tag_names = item_data.get('tagsString', '').split(',')
                    for tag_name in tag_names:
                        tag_name = tag_name.strip()
                        if not tag_name:
                            continue
                            
                        tag = self.env['inventory.connector.tag'].search([
                            ('inventory_id', '=', self.id),
                            ('name', '=', tag_name)
                        ], limit=1)
                        
                        if not tag:
                            tag = self.env['inventory.connector.tag'].create({
                                'name': tag_name,
                                'inventory_id': self.id,
                            })
                            _logger.info(f"Created new tag: {tag_name}")
                            
                        tag_ids.append(tag.id)
                
                # Apply tags to the item
                if tag_ids:
                    _logger.info(f"Applying {len(tag_ids)} tags to item {existing_item.name}")
                    existing_item.tag_ids = [(6, 0, tag_ids)]
            
            # Update last_sync
            self.write({
                'last_sync': now
            })
            
            return {
                'type': 'ir.actions.client',
                'tag': 'display_notification',
                'params': {
                    'title': _('Items Imported'),
                    'message': _('%s items imported, %s items updated') % (imported_count, updated_count),
                    'sticky': False,
                    'type': 'success',
                }
            }
            
        except Exception as e:
            _logger.error("Error importing inventory items: %s", str(e))
            raise UserError(_("Error importing inventory items: %s") % str(e))